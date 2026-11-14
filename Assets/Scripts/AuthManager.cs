using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    // UI elements for Signup
    public TMP_InputField signupUsernameInput;
    public TMP_InputField signupPasswordInput;
    public TMP_InputField signupConfirmPasswordInput;
    public TMP_Text signupMessageText;

    // UI elements for Login
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginMessageText;

    // UI panels to switch between on successful login
    public GameObject loginPanel; // Login panel
    public GameObject profilePanel; // New panel

    // Settings for running the Python script
    [Header("Python setup")]
    [SerializeField] private string pythonExecutable = "python"; 
    [SerializeField] private string scriptFileName = "ProfAuthentication.py"; // name of the script 
    [SerializeField] private int timeoutSeconds = 10; // how long to wait before timing out

    // Shape of the JSON sent to the Python script
    [Serializable]
    private class AuthRequest
    {
        public string username;
        public string password;
        public string confirmPassword;
    }

    // Shape of the JSON the Python script sends back
    [Serializable]
    private class AuthResponse
    {
        public string status;
        public string message;
    }

    // Method to handle Signup
    public void OnSignupButtonClicked()
    {
        string username = signupUsernameInput.text;
        string password = signupPasswordInput.text;
        string confirmPassword = signupConfirmPasswordInput.text;

        // Clear the previous message (if there are any)
        signupMessageText.text = "";

        // Check if passwords match before passing to Python script
        if (password != confirmPassword)
        {
            ShowMessage(signupMessageText, "Passwords do not match.");
            return;
        }

        // Run Python script for Signup
        AuthResponse response = RunAuthScript("signup", username, password, confirmPassword);

        // Display result message for Signup
        if (response.status == "success")
        {
            ShowMessage(signupMessageText, "Signup successful!");
            ClearInputs(signupUsernameInput, signupPasswordInput, signupConfirmPasswordInput); // empty the fields so the password is not left on screen
        }
        else
        {
            ShowMessage(signupMessageText, response.message); // the script explains what went wrong, so show that
        }
    }

    private IEnumerator ClearMessageAfterDelay(TMP_Text messageText, float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.text = "";
    }

    // Method to handle Login
    public void OnLoginButtonClicked()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        // Clear the previous message (if there are any)
        loginMessageText.text = "";

        // Run Python script for Login
        AuthResponse response = RunAuthScript("login", username, password, "");

        // Display result message for Login
        if (response.status == "success")
        {
            loginMessageText.text = "Login successful!";
            ClearInputs(loginUsernameInput, loginPasswordInput); // empty the fields so the password is not left on screen
            Invoke(nameof(ShowProfile), 2.0f); // Calls method after a 2 second delay
        }
        else
        {
            ShowMessage(loginMessageText, response.message);
        }
    }

    // Method to switch to profile menu on successful login
    private void ShowProfile()
    {
        // Deactivate the login panel
        loginPanel.SetActive(false);

        // Activate the new panel (Main Menu or Dashboard)
        profilePanel.SetActive(true);
    }

    // Method to show a message and hide it again after 3 seconds
    private void ShowMessage(TMP_Text target, string message)
    {
        target.text = message;
        StartCoroutine(ClearMessageAfterDelay(target, 3f)); // Hide message after 3 seconds
    }

    // Method to empty any number of input fields at once
    private void ClearInputs(params TMP_InputField[] fields)
    {
        foreach (TMP_InputField field in fields)
        {
            field.text = "";
        }
    }

    // Method to find which Python to run preferring the environment variable over the Inspector value
    private string PythonPath()
    {
        string fromEnvironment = Environment.GetEnvironmentVariable("JOURNEYLANES_PYTHON");
        return string.IsNullOrWhiteSpace(fromEnvironment) ? pythonExecutable : fromEnvironment;
    }

    // Method to run the Python script (both Signup and Login)
    private AuthResponse RunAuthScript(string action, string username, string password, string confirmPassword)
    {
        // path to the python script, found inside StreamingAssets so it ships with the build
        string scriptPath = Path.Combine(Application.streamingAssetsPath, scriptFileName);

        // Check the script is actually there before trying to run it
        if (!File.Exists(scriptPath))
        {
            UnityEngine.Debug.LogError("Auth script not found at: " + scriptPath);
            return Failure("Authentication is not set up correctly on this machine.");
        }

        // persistentDataPath is writable on every platform, unlike the install folder
        string databasePath = Path.Combine(Application.persistentDataPath, "credentials.db");

        // Construct the arguments for the process
        string arguments = $"\"{scriptPath}\" {action} \"{databasePath}\"";

        // Create the ProcessStartInfo object
        ProcessStartInfo psi = new ProcessStartInfo()
        {
            FileName = PythonPath(),
            Arguments = arguments,
            RedirectStandardInput = true, // the credentials are written here instead
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            
            using (Process process = Process.Start(psi))
            {
                // Send the credentials as JSON on standard input and closes when there isnt anymore to read
                AuthRequest request = new AuthRequest()
                {
                    username = username,
                    password = password,
                    confirmPassword = confirmPassword
                };

                process.StandardInput.Write(JsonUtility.ToJson(request));
                process.StandardInput.Close();

                // Read everything the script prints on two background threads
                // reading here directly would wait until the script finished, so the timeout below could never fire
                Task<string> outputTask = Task.Run(() => process.StandardOutput.ReadToEnd());
                Task<string> errorTask = Task.Run(() => process.StandardError.ReadToEnd());

                // Stop waiting if the script takes too long so the game does not freeze
                if (!process.WaitForExit(timeoutSeconds * 1000))
                {
                    process.Kill();
                    return Failure("The authentication service took too long to respond.");
                }

                // The script has finished, so both reads complete straight away
                string output = outputTask.Result;
                string errors = errorTask.Result;

                
                if (!string.IsNullOrWhiteSpace(errors))
                {
                    UnityEngine.Debug.LogWarning("Auth script stderr: " + errors);
                }

                // Return the output as the result
                return Parse(output);
            }
        }
        catch (Exception e)
        {
            // Fails if Python is missing or cannot be started
            UnityEngine.Debug.LogError("Could not run the auth script: " + e.Message);
            return Failure("Could not reach the authentication service.");
        }
    }

    // Method to turn the scripts JSON output into an AuthResponse
    private AuthResponse Parse(string output)
    {
        // Fails if there is no response at all
        if (string.IsNullOrWhiteSpace(output))
        {
            return Failure("No response from the authentication service.");
        }

        try
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(output.Trim());

            // Checks if JSON is in right format
            if (response == null || string.IsNullOrEmpty(response.status))
            {
                return Failure("Unexpected response from the authentication service.");
            }

            return response;
        }
        catch (Exception e)
        {
            // Catches if JSON was valid
            UnityEngine.Debug.LogError("Could not read auth response: " + e.Message + " | raw: " + output);
            return Failure("Unexpected response from the authentication service.");
        }
    }

    // Method to build an error response in same format as python script
    private AuthResponse Failure(string message)
    {
        return new AuthResponse() { status = "error", message = message };
    }
}
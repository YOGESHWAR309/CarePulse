<#
Set Twilio environment variables for the current PowerShell session.

Usage:
1. Edit the values for $TwilioSid, $TwilioToken and $TwilioFrom below.
2. Run this file in PowerShell:
   .\set-twilio-env.ps1
3. Restart the API if it is running.

Note: This sets variables only for the current session. To persist them for your user, uncomment the [Environment]::SetEnvironmentVariable lines.
Do NOT paste credentials into public chat.
#>

function Read-SecureValue([string]$prompt, [bool]$secure = $false) {
    if ($secure) {
        Write-Host $prompt -NoNewline
        $secureValue = Read-Host -AsSecureString
        return [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureValue))
    }
    return Read-Host $prompt
}

$TwilioSid = ""
$TwilioToken = ""
$TwilioFrom = ""

if ([string]::IsNullOrWhiteSpace($TwilioSid)) {
    $TwilioSid = Read-SecureValue "Enter Twilio Account SID: "
}
if ([string]::IsNullOrWhiteSpace($TwilioToken)) {
    $TwilioToken = Read-SecureValue "Enter Twilio Auth Token: " $true
}
if ([string]::IsNullOrWhiteSpace($TwilioFrom)) {
    $TwilioFrom = Read-SecureValue "Enter Twilio From Number (E.164 format, e.g. +919405355435): "
}

Write-Host "Setting Twilio environment variables for this PowerShell session..."
$env:Sms__TwilioAccountSid = $TwilioSid
$env:Sms__TwilioAuthToken = $TwilioToken
$env:Sms__FromNumber = $TwilioFrom

# Also set the alternate flat TWILIO_* names so the current Linux/PowerShell values work.
$env:TWILIO_ACCOUNT_SID = $TwilioSid
$env:TWILIO_AUTH_TOKEN = $TwilioToken
$env:TWILIO_FROM = $TwilioFrom

Write-Host "Done. These variables apply to the current PowerShell session only."
Write-Host "Restart the CarePulse API if it is running so it picks up the new variables."

# Persist for current user (uncomment to enable):
# [Environment]::SetEnvironmentVariable("Sms__TwilioAccountSid", $TwilioSid, "User")
# [Environment]::SetEnvironmentVariable("Sms__TwilioAuthToken", $TwilioToken, "User")
# [Environment]::SetEnvironmentVariable("Sms__FromNumber", $TwilioFrom, "User")
# [Environment]::SetEnvironmentVariable("TWILIO_ACCOUNT_SID", $TwilioSid, "User")
# [Environment]::SetEnvironmentVariable("TWILIO_AUTH_TOKEN", $TwilioToken, "User")
# [Environment]::SetEnvironmentVariable("TWILIO_FROM", $TwilioFrom, "User")

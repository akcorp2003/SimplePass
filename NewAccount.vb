Imports System
Imports System.Security
Imports System.Security.Cryptography
Imports System.Security.Cryptography.SHA1
Imports System.Security.SecureString
Imports System.Text

Public Class NewAccount



    Private Sub usercreate_Click(sender As Object, e As EventArgs) Handles usercreate.Click
        REM take the input and use the function provided in Form2 to insert the new user
        REM this is probably a place where I want to try to use RSA hash
        Dim textbytes As Byte()
        Dim encryptedpass As Byte()
        Dim encoder As New ASCIIEncoding
        Dim SHA As New SHA1CryptoServiceProvider()

        Dim myname As String = UserName.Text
        Dim mypass As String = userpass.Text

        textbytes = Encoding.ASCII.GetBytes(userpass.Text)
        encryptedpass = SHA.ComputeHash(textbytes)

        REM pass to function
        Dim success As Boolean = Form2.InsertUser(myname, mypass, encryptedpass)
        If success = True Then
            MsgBox("Your account has been successfully created!" & Environment.NewLine & "You may now login and enjoy SimplePass!")
            Me.Hide()
            UserName.Text = ""
            userpass.Text = ""
            REM And return the interface back to the login screen
            Form2.Show()
        Else
            MsgBox("Something went wrong with the creation of your account. We apologize for this. Try again.")
            UserName.Text = ""
            userpass.Text = ""
        End If
    End Sub

    Public Function UnicodeBytesToString(ByVal bytes() As Byte) As String
        Return System.Text.Encoding.Unicode.GetString(bytes)
    End Function
End Class
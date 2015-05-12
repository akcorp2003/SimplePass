Imports System
Imports System.Security
Imports System.Security.Cryptography
Imports System.Security.Cryptography.SHA1
Imports System.Security.SecureString
Imports System.Text

Public Class Form2
    Structure User
        Dim username As String
        Dim password As String
        Dim rsa As RSACryptoServiceProvider
        Dim encryptedPass As Byte()
    End Structure

    Structure Master_User
        Dim encryptedPass As Byte()
        Dim rsa As RSACryptoServiceProvider
    End Structure

    Dim usersonPC(10) As User
    Dim Pass_Master As Master_User
    Dim numUsers As Integer = 0
    Dim currentUser As Integer

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click
        NewAccount.Show()
    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        REM first, we will determine if we are loading a brand new instance of this program
        REM i.e. check if we have a masterfile on file

        Dim master_filename As String = "masterfile.txt"
        Dim master_filereader As System.IO.StreamReader
        Dim rsa As New RSACryptoServiceProvider()

        Try
            master_filereader = My.Computer.FileSystem.OpenTextFileReader(master_filename)
        Catch ex As Exception
            REM well, since we couldn't find one, then we must create a new RSA dude!
            Pass_Master.rsa = rsa
            Exit Try
        End Try

        If Not master_filereader Is Nothing Then
            Pass_Master.rsa = rsa
            REM we have located the file, now we can initialize the RSA object
            Dim masterdata_array() As String
            Dim master_linecontents As String
            master_linecontents = master_filereader.ReadLine()
            While master_filereader.EndOfStream() = False
                masterdata_array = Split(master_linecontents, " ")
                If String.Equals(masterdata_array(0), "END") Then
                    Exit While
                End If
                If String.Equals(masterdata_array(0), "XML:") Then
                    InitializeMaster(masterdata_array)
                End If
                master_linecontents = master_filereader.ReadLine()
            End While
            master_filereader.Close()
        End If

        REM we will read in the contents of a file
        Dim filename As String = "initialization.txt"

        Dim filereader As System.IO.StreamReader
        Try
            filereader = My.Computer.FileSystem.OpenTextFileReader(filename)
        Catch ex As Exception
            REM if we get here, then there is no file created. We simply return
            Return
        End Try

        Dim userinfo(4) As String
        Dim passblock(3) As String
        Dim curr_workinguser As Integer = -1
        Dim linecontents As String
        Dim passwordbytes(128) As Byte
        Dim passwordbytes0(128) As Byte
        linecontents = filereader.ReadLine()

        While filereader.EndOfStream() = False
            REM first tokenize the line
            Dim linecontents_array() As String = Split(linecontents, " ")
            REM by design, we know that this array's first element indicates what data the second element represents
            Dim elementIdentifier As String = linecontents_array(0)
            Dim elementIdentifier_int As Integer = Get_elementIdentifier(elementIdentifier)


            If elementIdentifier_int = 0 Then
                REM this will be the SimplePass username
                userinfo(0) = linecontents_array(1)
                curr_workinguser += 1
            ElseIf elementIdentifier_int = 1 Then
                REM this will be the SimplePass encrypted password
                userinfo(1) = linecontents_array(1)
                REM at this point, all the user's information is ready
                InitializeUser(userinfo)
                REM the below condition loop is outdated...
            ElseIf elementIdentifier_int = 2 Then
                REM this will be the keys
                userinfo(2) = linecontents_array(1)


            ElseIf elementIdentifier_int = 3 Then
                REM this will be the site username
                passblock(0) = linecontents_array(1)
            ElseIf elementIdentifier_int = 4 Then
                REM this will be the password hash
                passblock(1) = linecontents_array(1)
            ElseIf elementIdentifier_int = 5 Then
                passblock(2) = linecontents_array(1)
                REM in order to insert the password into InsertPassNode, we have to decrypt it and pass
                REM it in as Byte format
                passwordbytes = Pass_Master.rsa.Decrypt(Convert.FromBase64String(passblock(1)), True)
                REM at this point, all the current PassBlock's information is ready
                Form1.listofUsers(curr_workinguser).UserTree.InsertPassNode(passblock(0), passwordbytes, passblock(2))
            End If
            linecontents = filereader.ReadLine()
        End While
        filereader.Close()
    End Sub

    REM the return values are as follows:
    REM USER: ->0
    REM HASH: ->1
    REM XML:  ->2
    REM USERNAME: ->3
    REM PASSWORD: ->4
    REM PROPERTY: ->5
    Private Function Get_elementIdentifier(ByVal element As String) As Integer
        Dim elementArray() As String = {"USER:", "HASH:", "XML:", "USERNAME:", "PASSWORD:", "PROPERTY: "}
        Dim returnresult As Integer = -2000
        Dim index As Integer = 0
        For Each Str As String In elementArray
            If Str.Contains(element) Then
                Exit For
            End If
            index += 1
        Next
        Return index
    End Function

    Private Function InitializeUser(ByVal userinfo_array() As String) As Boolean
        Dim passwordbytes() As Byte

        passwordbytes = Convert.FromBase64String(userinfo_array(1))
        InsertUser(userinfo_array(0), userinfo_array(1), passwordbytes)
        Return True
    End Function

    Private Function InitializeMaster(ByRef masterinfo_array() As String) As Boolean
        Try
            Pass_Master.rsa.FromXmlString(masterinfo_array(1))
        Catch ex As Exception

        End Try

        Return True
    End Function

    Private Sub Form2_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) _
        Handles Me.FormClosed
        MsgBox("Thank you for using SimplePass")

        REM we are first going to write down the master key information and password hash
        Dim masterfilename As String = "masterfile.txt"
        Try
            My.Computer.FileSystem.DeleteFile(masterfilename)
        Catch ex As System.IO.FileNotFoundException
            REM hehe...nothing to do...
        End Try

        Dim data As String
        data = Nothing

        data = "XML: " + Pass_Master.rsa.ToXmlString(True) + Environment.NewLine
        My.Computer.FileSystem.WriteAllText(masterfilename, data, True)
        My.Computer.FileSystem.WriteAllText(masterfilename, "END", True)

        Dim i As Integer
        Dim encoder As New UTF8Encoding
        Dim filename As String = "initialization.txt"
        Try
            My.Computer.FileSystem.DeleteFile(filename)
        Catch ex As System.IO.FileNotFoundException
            REM there is seriously nothing to do if there is no file found...
        End Try

        For i = 0 To numUsers - 1 Step 1
            REM first record the username. This is what we will use to know that we are on a new user
            REM when we initialize SimplePass
            data = "USER: " + usersonPC(i).username + Environment.NewLine
            My.Computer.FileSystem.WriteAllText(filename, data, True)
            REM next will be the password hash
            data = "HASH: " + Convert.ToBase64String(usersonPC(i).encryptedPass) + Environment.NewLine
            My.Computer.FileSystem.WriteAllText(filename, data, True)

            REM now will be the fun stuff
            REM we will write all the usernames and passwords stored by this user
            Dim passnodes_to_print As New PassQueue()
            passnodes_to_print = Form1.listofUsers(i).UserTree.Collect()
            If passnodes_to_print Is Nothing Then
                REM the user never stored anything...
                My.Computer.FileSystem.WriteAllText(filename, "END", True)
                Return
            End If
            REM now that we have all the PassNodes, we can print them to paper!
            Dim currblock As QueueBlock
            currblock = passnodes_to_print.Top()
            While Not currblock Is Nothing
                REM first print username
                data = "USERNAME: " + currblock.data.username + Environment.NewLine
                My.Computer.FileSystem.WriteAllText(filename, data, True)
                REM then password
                data = "PASSWORD: " + Convert.ToBase64String(currblock.data.password) + Environment.NewLine
                My.Computer.FileSystem.WriteAllText(filename, data, True)
                REM then property
                data = "PROPERTY: " + currblock.data.my_property + Environment.NewLine
                My.Computer.FileSystem.WriteAllText(filename, data, True)

                REM now check to see if currblock has any Brothers

                Dim currqueueblock As New QueueBlock
                Dim index As Integer = 0
                currqueueblock = currblock.data.Brothers.Top()
                While index < currblock.data.Brothers.numElements()
                    data = "USERNAME: " + currqueueblock.data.username + Environment.NewLine
                    My.Computer.FileSystem.WriteAllText(filename, data, True)
                    data = "PASSWORD: " + Convert.ToBase64String(currqueueblock.data.password) + Environment.NewLine
                    My.Computer.FileSystem.WriteAllText(filename, data, True)
                    data = "PROPERTY: " + currqueueblock.data.my_property + Environment.NewLine
                    My.Computer.FileSystem.WriteAllText(filename, data, True)
                    currqueueblock = currqueueblock.nextblock
                    index += 1
                End While

                passnodes_to_print.Pop()
                currblock = passnodes_to_print.Top()

            End While
        Next

        My.Computer.FileSystem.WriteAllText(filename, "END", True)

    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim tempuser As String
        Dim temppass As String
        Dim candidatePass As String
        Dim textbytes As Byte()
        Dim encrypted_candidateuserpass As Byte()
        Dim encoder As New ASCIIEncoding
        Dim SHA As New SHA1CryptoServiceProvider()

        tempuser = UserName.Text

        REM begin encryption of password text
        temppass = userpass.Text
        textbytes = Encoding.ASCII.GetBytes(userpass.Text)
        encrypted_candidateuserpass = SHA.ComputeHash(textbytes)
        candidatePass = Encoding.ASCII.GetString(encrypted_candidateuserpass)

        REM do a search in our array
        For index As Integer = 0 To UBound(usersonPC) Step 1
            If usersonPC(index).username = tempuser Then
                REM verify the hashed version of the given password with the password stored on file
                Dim pass_on_file As String
                pass_on_file = Encoding.ASCII.GetString(usersonPC(index).encryptedPass)
                If String.Equals(candidatePass, pass_on_file) = True Then
                REM we have successfully opened the user so we will open our main form
                currentUser = index
                UserName.Text = ""
                userpass.Text = ""
                Me.Hide()
                Dashboard.Show()
                Return
            End If
            End If
        Next
        REM if we reach here, we did not find the user
        MsgBox("Either the user does not exist or the passwords did not match. Please reenter.")
        UserName.Text = ""
        userpass.Text = ""
    End Sub

    Public Function InsertUser(ByVal name As String, ByVal password As String, _
                               ByVal encryptedPass As Byte()) As Boolean
        Dim tempuserid As Integer = numUsers
        If numUsers = 0 Then
            REM insert at the top of the array
            usersonPC(numUsers).username = name
            usersonPC(numUsers).password = password
            usersonPC(numUsers).encryptedPass = encryptedPass
        ElseIf numUsers >= 10 Then
            Return False
        Else
            usersonPC(numUsers).username = name
            usersonPC(numUsers).password = password
            usersonPC(numUsers).encryptedPass = encryptedPass
        End If
        REM fetch current user
        numUsers += 1
        REM now we shall initialize an entry in Form1's listofUsers array
        Form1.listofUsers(tempuserid).UserID = tempuserid
        Form1.listofUsers(tempuserid).UserTree = New PassTree()

        Return True
    End Function

    Public Function FetchCurrentUser() As Integer
        Return currentUser
    End Function

    Public Function FetchCurrentUserRSAObject() As RSACryptoServiceProvider
        Return usersonPC(currentUser).rsa
    End Function

    Public Function FetchMasterRSAObject() As RSACryptoServiceProvider
        Return Pass_Master.rsa
    End Function

    Public Function ResetUser() As Boolean
        currentUser = -1
        Return Nothing
    End Function


    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click
        AboutSimplePass.Show()
    End Sub
End Class
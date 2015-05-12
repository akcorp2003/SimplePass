Imports System.Security
Imports System.Security.Cryptography
Imports System.Security.SecureString
Imports System.Text

Public Class SearchPasswords

    Private Sub ClearBoxes()
        UserSearch.Text = ""
        SiteNameSearch.Text = ""
    End Sub

    Private Sub SearchPasswords_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) _
        Handles Me.FormClosed
        Me.Hide()
        Dashboard.Show()
    End Sub

    Private Sub Search_Click(sender As Object, e As EventArgs) Handles Search.Click
        Dim curruserID As Integer = Form2.FetchCurrentUser()
        REM check to see which box is blank 
        If String.IsNullOrWhiteSpace(SiteNameSearch.Text) = True Then
            Dim fetchedResult As PassNode = Form1.listofUsers(curruserID).UserTree.FindUsername(UserSearch.Text)
            If Not fetchedResult Is Nothing Then
                REM check to see if there are any Brothers associated to this username
                If fetchedResult.Brothers.IsEmpty() = True Then
                    Dim displaypass As String
                    Dim passbytes As Byte()
                    Dim encoder As New UTF8Encoding
                    Dim rsa As RSACryptoServiceProvider
                    rsa = Form2.FetchMasterRSAObject()
                    passbytes = rsa.Decrypt(fetchedResult.password, True)
                    displaypass = encoder.GetString(passbytes)
                    MsgBox("Your password for the username " & UserSearch.Text & " is:" & Environment.NewLine _
                       & displaypass & ". " & Environment.NewLine & "The site associated to " & UserSearch.Text & " is: " _
                       & fetchedResult.my_property & ".")
                Else
                    REM we have some Brothers on our hands...
                    Dim outputstring = ""
                    outputstring = AssembleIntoFormation(Nothing, fetchedResult)
                    MsgBox("Here is the list of passwords for your username " & UserSearch.Text & ": " & Environment.NewLine _
                        & outputstring)
                End If

                Me.ClearBoxes()
            Else
                MsgBox("This username does not exist. Please reenter.")
                Me.ClearBoxes()
            End If
        ElseIf String.IsNullOrWhiteSpace(UserSearch.Text) = True Then
            REM for now, we are going to do a very expensive search by going through the entire list of usernames
            REM and gathering a queue of them to send back here
            Dim resultblock As New QueueBlock
            Dim outputstring = ""
            resultblock = Form1.listofUsers(curruserID).UserTree.FindSiteName(SiteNameSearch.Text)
            If resultblock Is Nothing Then
                MsgBox("This site does not exist. Please reenter.")
            Else
                outputstring = AssembleIntoFormation(resultblock, Nothing)
                MsgBox("Here is the list of Usernames and Passwords for the site " & SiteNameSearch.Text & ": " & Environment.NewLine _
                       & outputstring)
            End If

            Me.ClearBoxes()
        End If

    End Sub

    Private Function AssembleIntoFormation(ByVal informationblock As QueueBlock, ByVal informationnode As PassNode) As String
        Dim returnstring As String = ""
        Dim counter As Integer = 1
        Dim liststring As String = ""

        Dim rsa As RSACryptoServiceProvider
        Dim encoder As New UTF8Encoding
        Dim displaypass As String
        Dim passbytes As Byte()

        rsa = Form2.FetchMasterRSAObject()


        If informationblock Is Nothing Then
            REM then we have been requested to assemble informationnode
            REM this means that we have to consider the Brothers
            liststring = Convert.ToString(counter)
            passbytes = rsa.Decrypt(informationnode.password, True)
            displaypass = encoder.GetString(passbytes)
            returnstring = liststring + ". " + informationnode.username + ", " + displaypass + ", " + informationnode.my_property + Environment.NewLine
            counter += 1
            If informationnode.Brothers.IsEmpty() = False Then
                Dim tempblock As New QueueBlock
                tempblock = informationnode.Brothers.Top()
                While Not tempblock Is Nothing
                    liststring = Convert.ToString(counter)
                    passbytes = rsa.Decrypt(tempblock.data.password, True)
                    displaypass = encoder.GetString(passbytes)
                    returnstring = returnstring + liststring + ". " + tempblock.data.username + ", " + displaypass + ", " + tempblock.data.my_property + Environment.NewLine
                    REM move on to the next thing
                    tempblock = tempblock.nextblock
                    counter += 1
                End While
            End If
        Else
            REM we will be operating on informationblock
            Dim tempblock As New QueueBlock
            tempblock = informationblock.Top()
            Dim index As Integer = 0
            While index < informationblock.numElements()
                liststring = Convert.ToString(counter)
                passbytes = rsa.Decrypt(tempblock.data.password, True)
                displaypass = encoder.GetString(passbytes)
                returnstring = returnstring + liststring + ". " + tempblock.data.username + ", " + displaypass + ", " + tempblock.data.my_property + Environment.NewLine
                tempblock = tempblock.nextblock
                counter += 1
                index += 1
            End While
        End If
        Return returnstring
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Hide()
        Dashboard.Show()
    End Sub
End Class
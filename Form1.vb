Imports System
Imports System.Security
Imports System.Security.Cryptography
Imports System.Security.SecureString
Imports System.Text

Public Class Form1

    Private Sub ClearBoxes()
        UserName.Text = ""
        Password.Text = ""
        SiteName1.Text = ""
        UserName2.Text = ""
        SiteName2.Text = ""
    End Sub

    Structure UserBlock
        Dim UserTree As PassTree
        Dim UserID As Integer
    End Structure

    Public listofUsers(10) As UserBlock

    Private Sub ADD_Click(sender As Object, e As EventArgs) Handles ADD.Click
        Dim curruserid As Integer = Form2.FetchCurrentUser()
        REM first encrypt the password
        Dim textbytes As Byte()
        Dim encoder As New UTF8Encoding
        Dim insertionresult As Boolean
        textbytes = encoder.GetBytes(Password.Text)
        insertionresult = listofUsers(curruserid).UserTree.InsertPassNode(UserName.Text, textbytes, SiteName1.Text)
        If insertionresult = True Then
            MessageBox.Show(UserName.Text & " was successfully added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("Unfortunately, we could not add " & UserName.Text & ". Something probably went wrong on our end.", _
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End If
        Me.ClearBoxes()
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) _
        Handles Me.FormClosed
        Me.Hide()
        Dashboard.Show()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim userconfirm As DialogResult
        userconfirm = MessageBox.Show("Are you sure you want to delete the username and password for: " & _
                        UserName2.Text & "?", "Deletion Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If userconfirm = vbNo Then
            MsgBox("We did not delete any data. :-)")
            ClearBoxes()
        Else
            REM deletion is the classic 3 cases for a binary tree
            REM first get the PassNode associated to the search request
            Dim curruser = Form2.FetchCurrentUser()
            REM listofUsers(curruser).UserTree.RemovePassNode_Simple(UserName2.Text)
            If String.IsNullOrWhiteSpace(UserName2.Text) = False And String.IsNullOrWhiteSpace(SiteName2.Text) = False Then
                REM the user requested a single deletion
                Dim deletionresult As Boolean
                deletionresult = listofUsers(curruser).UserTree.RemovePassNode_SingleUserName(UserName2.Text, SiteName2.Text)

                If deletionresult = True Then
                    MessageBox.Show("We have successfully deleted the username " & UserName2.Text & ".", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    MessageBox.Show("Something went wrong when attempting to delete your username. Please try again.", "Failure", _
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            ElseIf String.IsNullOrWhiteSpace(SiteName2.Text) = True Then
                REM user requested to delete all associations to the username
                userconfirm = MessageBox.Show("Are you sure you want to delete everything about the username " & UserName2.Text & _
                                              "?", "Deletion Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If userconfirm = vbNo Then
                    MsgBox("We did not delete any data. :-)")
                    ClearBoxes()
                Else
                    Dim returnresult As Boolean
                    returnresult = listofUsers(curruser).UserTree.RemovePassNode_AllUserName(UserName2.Text)
                    If returnresult = True Then
                        MessageBox.Show("We have successfully deleted the username " & UserName2.Text & ".", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        MessageBox.Show("Something went wrong when attempting to delete your username. Please try again.", "Failure", _
                                        MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End If
            End If
            ClearBoxes()
        End If

    End Sub


    Private Sub ReturnToDash_Click(sender As Object, e As EventArgs) Handles ReturnToDash.Click
        Me.Hide()
        Dashboard.Show()
    End Sub

    Private Sub ToolTip1_Popup(sender As Object, e As PopupEventArgs) Handles SiteNameHelpTip.Popup

    End Sub
End Class





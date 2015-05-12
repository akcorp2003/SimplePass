Public Class Dashboard

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        SearchPasswords.Show()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Hide()
        Form1.Show()
    End Sub

    Private Sub LOGOUT_Click(sender As Object, e As EventArgs) Handles LOGOUT.Click
        Form2.ResetUser()
        Me.Hide()
        Form2.Show()
    End Sub

    Private Sub Dashboard_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) _
        Handles Me.FormClosed
        Form2.Close()
    End Sub
End Class
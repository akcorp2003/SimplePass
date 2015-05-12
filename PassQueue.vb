Public Class PassQueue
    Public head As QueueBlock
    Public tail As QueueBlock
    Public num_items As Integer = 0

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public Function InsertItem(ByVal Node As PassNode) As Boolean
        Dim newblock As New QueueBlock()
        newblock.nextblock = Nothing
        newblock.data = Node
        If head Is Nothing Then
            REM there is nothing in the queue...
            head = newblock
            tail = newblock
            num_items += 1
            Return True
        Else
            tail.nextblock = newblock
            tail = newblock
            num_items += 1
            Return True
        End If
        Return False
    End Function

    Public Function Contains(ByVal username As String) As Boolean
        Dim tempblock As New QueueBlock()
        tempblock = Me.Top()
        While Not tempblock Is Nothing
            Me.Pop()
            If String.Equals(username, tempblock.data.username) = True Then
                Return True
            Else
                tempblock = Me.Top()
            End If
        End While
        Return False
    End Function

    Public Function Pop() As Boolean
        Dim temp As QueueBlock
        temp = head
        head = temp.nextblock
        num_items -= 1
        Return True
    End Function

    Public Function Top() As QueueBlock
        Return head
    End Function

    Public Function IsEmpty() As Boolean
        If head Is Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
End Class



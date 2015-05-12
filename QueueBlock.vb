Public Class QueueBlock
    Public nextblock As QueueBlock
    Public data As PassNode

    Public head As QueueBlock
    Public tail As QueueBlock

    Private num_elements As Integer = 0

    Public Function InsertItem(ByVal element As QueueBlock) As Boolean
        If Me.IsEmpty() = True Then
            head = element
            tail = element
            num_elements += 1
            Return True
        Else
            tail.nextblock = element
            element.nextblock = Nothing
            tail = element
            num_elements += 1
            Return True
        End If
        Return False
    End Function

    Public Function DeleteTop() As Boolean
        If num_elements = 1 Then
            REM this is fear of when we reference Brothers, we may be referencing a null item
            num_elements -= 1
        Else
            head = head.nextblock
        End If
        Return True
    End Function

    REM field = 1 -> look by username
    REM field = 2 -> look by password
    REM field = 3 -> look by my_property
    Public Function DeleteElement(ByVal deleterequest As String, ByVal field As Integer) As Boolean
        Dim iterator_block As New QueueBlock
        Dim delayed_block As New QueueBlock
        Dim deletionrequest As Boolean = False

        iterator_block = head

        REM first check if the requested deletion is the head
        If field = 1 Then
            If String.Equals(iterator_block.data.username, deleterequest) = True Then
                Me.DeleteTop()
                Return True
            End If
        ElseIf field = 2 Then
            If String.Equals(iterator_block.data.password, deleterequest) = True Then
                Me.DeleteTop()
                Return True
            End If
        Else
            If String.Equals(iterator_block.data.my_property, deleterequest) = True Then
                Me.DeleteTop()
                Return True
            End If

        End If
        REM if we get here, it looks like we're not deleting the head...

        delayed_block = iterator_block
        iterator_block = iterator_block.nextblock
        While Not iterator_block Is Nothing
            If field = 1 Then
                If String.Equals(iterator_block.data.username, deleterequest) = True Then
                    deletionrequest = True
                    Exit While
                End If
            ElseIf field = 2 Then
                If String.Equals(iterator_block.data.password, deleterequest) = True Then
                    deletionrequest = True
                    Exit While
                End If
            Else
                If String.Equals(iterator_block.data.my_property, deleterequest) = True Then
                    deletionrequest = True
                    Exit While
                End If
            End If
            delayed_block = iterator_block
            iterator_block = iterator_block.nextblock
        End While
        If deletionrequest = True Then
            delayed_block.nextblock = iterator_block.nextblock
            num_elements -= 1
            REM check if our deleted element is the tail
            If iterator_block.nextblock Is Nothing Then
                tail = delayed_block
            End If
            Return True
        End If
        Return False
    End Function

    Public Function IsEmpty() As Boolean
        If num_elements = 0 Then
            Return True
        End If
        Return False
    End Function

    Public Function numElements() As Integer
        Return num_elements
    End Function

    Public Function IsHeadNothing() As Boolean
        If head Is Nothing Then
            Return True
        End If
        Return False
    End Function

    Public Function Top() As QueueBlock
        If Me.IsEmpty() Then
            Return Nothing
        End If
        Return head
    End Function

    REM operation = 1 -> look for username
    REM operation = 2 -> look for password
    REM operation = 3 -> look for my_property
    Public Function Contains(ByVal value As String, ByVal operation As Integer) As QueueBlock
        Dim myiterator As New QueueBlock
        Dim caughtelement As New QueueBlock
        myiterator = Me.Top()
        While Not myiterator Is Nothing
            If operation = 1 Then
                If String.Equals(myiterator.data.username, value) = True Then
                    caughtelement.InsertItem(myiterator)
                End If
            ElseIf operation = 2 Then
                If String.Equals(myiterator.data.password, value) = True Then
                    caughtelement.InsertItem(myiterator)
                End If
            Else
                If String.Equals(myiterator.data.my_property, value) = True Then
                    caughtelement.InsertItem(myiterator)
                End If
            End If
            myiterator = myiterator.nextblock
        End While
        Return caughtelement
    End Function
End Class


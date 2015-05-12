Imports System
Imports System.Collections

Public Class PassNode
    REM although it's called a node, it is designed specifically for this application
    Public LeftNode As PassNode
    Public RightNode As PassNode
    Public Parent As PassNode
    Public Brothers As New QueueBlock()

    Public username As String
    Public password As Byte()
    Public my_property As String
End Class
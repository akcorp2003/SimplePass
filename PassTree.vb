Imports System
Imports System.Security
Imports System.Security.Cryptography
Imports System.Security.SecureString
Imports System.Text

Public Class PassTree
    REM This tree always has the greater value (username) on the right node and lesser on the left.

    Dim PassTree_root As PassNode

    Public Function InsertPassNode(ByVal name As String, ByVal pass() As Byte, ByVal my_property As String) As Boolean
        Dim encryptedpass As Byte()
        Dim rsa As RSACryptoServiceProvider
        Dim curruser As Integer
        curruser = Form2.FetchCurrentUser()

        rsa = Form2.FetchMasterRSAObject()
        encryptedpass = rsa.Encrypt(pass, True)
        Dim my_passnode As PassNode = CreatePassNode(name, encryptedpass, my_property)
        If PassTree_root Is Nothing Then
            REM we have an empty tree! This will be easy...
            REM set the new node as the root
            PassTree_root = my_passnode
            PassTree_root.LeftNode = Nothing
            PassTree_root.RightNode = Nothing
            PassTree_root.Parent = Nothing
            Return True
        Else
            REM now here's where things get tricky...we'll have to loop through and find the right
            REM place to put this new guy
            Dim tempnode As PassNode = PassTree_root
            While Not tempnode Is Nothing
                REM compare the current node's value and determine which node to explore
                Dim stringcompresult = String.Compare(my_passnode.username, tempnode.username)
                If stringcompresult < 0 Then
                    REM we have that name is less than tempnode.username
                    REM so the node belongs to the left side
                    REM investigate if the left side is occupied. If not, yay! If so, continue exploring the left
                    If tempnode.LeftNode Is Nothing Then
                        my_passnode.LeftNode = Nothing
                        my_passnode.RightNode = Nothing
                        my_passnode.Parent = tempnode
                        tempnode.LeftNode = my_passnode
                        Return True
                    Else
                        REM set the new tempnode to be the left of the current tempnode
                        tempnode = tempnode.LeftNode
                        Continue While
                    End If
                ElseIf stringcompresult > 0 Then
                    REM we have that name is greater than tempnode.username
                    REM so the node belongs to the right side
                    REM apply same thing we did as the left side
                    If tempnode.RightNode Is Nothing Then
                        my_passnode.LeftNode = Nothing
                        my_passnode.RightNode = Nothing
                        my_passnode.Parent = tempnode
                        tempnode.RightNode = my_passnode
                        Return True
                    Else
                        tempnode = tempnode.RightNode
                        Continue While
                    End If
                Else
                    REM we have equal strings!
                    REM this is the interesting case...

                    REM first check if the sites are the same
                    If String.Equals(my_passnode.my_property, tempnode.my_property) = True Then
                        Dim my_queueblock As New QueueBlock
                        my_queueblock.data = my_passnode
                        REM ask the user if they are updating the username for the existing site
                        Dim userconfirm As DialogResult
                        userconfirm = MessageBox.Show("Are you updating the username and password for this site " & tempnode.my_property & "?", _
                                                      "Duplication Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If userconfirm = vbNo Then
                            REM user has another account with the same username on this site...
                            tempnode.Brothers.InsertItem(my_queueblock)
                            Return True
                        Else
                            REM user wants to update! 
                            REM first check if the tempnode has any Brothers
                            If tempnode.Brothers.IsEmpty() = True Then
                                REM then we are just updating the tempnode's information, easy
                                tempnode.username = my_passnode.username
                                tempnode.password = my_passnode.password
                            Else
                                REM we will have to iterate through the entire list of Brothers to find
                                REM we have to find the passnode that contains the username
                                Dim tempblock As New QueueBlock
                                tempblock = tempnode.Brothers.Top()
                                While Not tempblock Is Nothing
                                    If String.Equals(tempblock.data.username, my_passnode.username) = True Then
                                        tempblock.data.username = my_passnode.username
                                        tempblock.data.password = my_passnode.password
                                        Return True
                                    Else
                                        tempblock = tempblock.nextblock
                                    End If
                                End While
                            End If
                        End If

                    Else
                        REM the user wants to add a new Brother!
                        Dim tempblock As New QueueBlock
                        tempblock.data = my_passnode
                        tempnode.Brothers.InsertItem(tempblock)

                    End If
                    Return True
                End If
            End While

        End If
        MsgBox("Whoopsy!")
        Return False
    End Function

    REM this function may not be in use in future release of this program
    Public Function RemovePassNode_Simple(ByVal searchquery As String) As Boolean
        Dim curruser As Integer = Form2.FetchCurrentUser()
        Dim candidatenode As PassNode = Form1.listofUsers(curruser).UserTree.FindUsername(searchquery)
        Dim returnval As Boolean = False
        If candidatenode Is Nothing Then
            returnval = False
        Else
            REM now it is onto the 3 classic cases of binary node deletion
            If candidatenode.LeftNode Is Nothing And candidatenode.RightNode Is Nothing Then
                REM check to see if we are dealing with the root node. If so, deletion will be pretty easy
                If candidatenode.Parent Is Nothing Then
                    Dim dummynode As PassNode = Nothing REM this variable is just used to pass into my root function
                    REM check for any Brothers in the root node
                    If candidatenode.Brothers.IsEmpty() = True Then
                        Form1.listofUsers(curruser).UserTree.SetUserPassTreeRoot(dummynode)
                        returnval = True
                    Else
                        Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                        returnval = True
                    End If

                Else
                    REM we will have to check which side of the parent this candidatenode is on
                    Dim compresult As Boolean = String.Equals(candidatenode.username, candidatenode.Parent.LeftNode.username)

                    If compresult = True Then
                        REM obviously we're on the left side. So we unlink the parent's left node
                        REM but first, we must check if candidatenode has any Brothers
                        If candidatenode.Brothers.IsEmpty() = True Then
                            candidatenode.Parent.LeftNode = Nothing
                            returnval = True
                        Else
                            Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                            returnval = True
                        End If

                    Else
                        REM obviously we're on the right side
                        If candidatenode.Brothers.IsEmpty() = True Then
                            candidatenode.Parent.RightNode = Nothing
                            returnval = True
                        Else
                            Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                            returnval = True
                        End If

                    End If
                End If

                REM cases with one children
            ElseIf candidatenode.LeftNode Is Nothing And Not candidatenode.RightNode Is Nothing Then
                REM figure out if we are dealing with the root node
                If candidatenode.Parent Is Nothing Then
                    REM pretty easy. Just associate the root to the candidate's right node!
                    If candidatenode.Brothers.IsEmpty() = True Then
                        PassTree_root = candidatenode.RightNode
                        returnval = True
                    Else
                        Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                        returnval = True
                    End If

                    Return returnval
                End If
                REM once again, figure out if we're on the left or right side of the parent
                Dim compresult As Boolean = String.Equals(candidatenode.username, candidatenode.Parent.LeftNode.username)
                If compresult = True Then
                    REM candidate is on left side!
                    REM so we will hook up the left arm of the parent to the right arm of the candidate node
                    If candidatenode.Brothers.IsEmpty() = True Then
                        candidatenode.Parent.LeftNode = candidatenode.RightNode
                        candidatenode.RightNode = Nothing
                        returnval = True
                    Else
                        Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                        returnval = True
                    End If

                Else
                    REM candidate is on right side!
                    If candidatenode.Brothers.IsEmpty() = True Then
                        candidatenode.Parent.RightNode = candidatenode.RightNode
                        returnval = True
                    Else
                        Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                        returnval = True
                    End If

                End If
            ElseIf Not candidatenode.LeftNode Is Nothing And candidatenode.RightNode Is Nothing Then
                REM figure out if we are dealing with the root node
                If candidatenode.Parent Is Nothing Then
                    REM pretty easy. Just associate the root to the candidate's left node!
                    PassTree_root = candidatenode.LeftNode
                    returnval = True
                    Return returnval
                End If
                REM relatively the same procedure as up top
                Dim compresult As Boolean = String.Equals(candidatenode.username, candidatenode.Parent.LeftNode.username)
                If compresult = True Then
                    REM candidate is on the left side!
                    REM we will hook up the left arm of the parent to the left arm of the candidate node
                    If candidatenode.Brothers.IsEmpty() = True Then
                        candidatenode.Parent.LeftNode = candidatenode.LeftNode
                        candidatenode.LeftNode = Nothing
                        returnval = True
                    Else
                        Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                        returnval = True
                    End If

                Else
                    REM candidate is on the right side!
                    If candidatenode.Brothers.IsEmpty() = True Then
                        candidatenode.Parent.RightNode = candidatenode.LeftNode
                        candidatenode.LeftNode = Nothing
                        returnval = True
                    Else
                        Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                        returnval = True
                    End If

                End If

                REM cases with 2 children (the trickiest)
            ElseIf Not candidatenode.LeftNode Is Nothing And Not candidatenode.RightNode Is Nothing Then

                If candidatenode.Brothers.IsEmpty() = False Then
                    REM this is a pretty easy case
                    REM we simply move the top brother into the passnode and be on our way
                    Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                Else
                    REM this is the more difficult case
                    REM we will pick the algorithm that will take the smallest value from the right subtree
                    Dim minNode As PassNode = Form1.listofUsers(curruser).UserTree.FindMinPassNode_onright(candidatenode)
                    REM now, the candidatenode will not be erased! We will simply replace its values
                    candidatenode.password = minNode.password
                    candidatenode.username = minNode.username
                    REM remove the associations to minNode
                    minNode.Parent = Nothing

                    REM now we begin some interesting stuff
                    REM we will look at if candidatenode and minNode have Brothers
                    REM we have already dealt with the case when candidatenode has Brothers
                    REM now, we just merge candidatenode's Brother with minNode's Brother
                    Form1.listofUsers(curruser).UserTree.MergeBrothers(candidatenode, minNode)

                    Dim rightTree As PassNode = minNode.RightNode
                    REM this is for the case when there are still some nodes hanging on our minNode's right side
                    REM our plan is to reinsert these guys back to where they belong
                    minNode.LeftNode = Nothing
                    minNode.RightNode = Nothing
                    Form1.listofUsers(curruser).UserTree.ReinsertSubTree(rightTree)
                End If

                returnval = True
                Return returnval
            End If
        End If REM end large IF
        candidatenode.username = ""
        Array.Clear(candidatenode.password, candidatenode.password.GetLowerBound(0), candidatenode.password.Length)
        candidatenode.my_property = ""
        Return returnval
    End Function

    Public Function RemovePassNode_SingleUserName(ByVal username As String, ByVal sitename As String) As Boolean
        Dim curruser As Integer = Form2.FetchCurrentUser()
        Dim candidatenode As PassNode = Form1.listofUsers(curruser).UserTree.FindUsername(username)
        If candidatenode Is Nothing Then
            Return False
        End If
        If candidatenode.Brothers.IsEmpty() = True Then
            REM this is just the same as deleting all the usernames for the request
            Dim result As Boolean = Form1.listofUsers(curruser).UserTree.RemovePassNode_AllUserName(candidatenode.username)
            Return result
        Else
            REM all we need to do is locate the value in candidatenode that has the same sitename and remove it!
            If String.Equals(candidatenode.my_property, sitename) = True Then
                Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                Return True
            Else
                Dim deletionresult As Boolean
                deletionresult = candidatenode.Brothers.DeleteElement(sitename, 3)
                If deletionresult = True Then
                    Return True
                Else
                    Return False
                End If
            End If
        End If
        Return False
    End Function

    Public Function RemovePassNode_AllUserName(ByVal username As String) As Boolean
        Dim curruser As Integer = Form2.FetchCurrentUser()
        Dim candidatenode As PassNode = Form1.listofUsers(curruser).UserTree.FindUsername(username)
        Dim returnval As Boolean = False
        If candidatenode Is Nothing Then
            returnval = False
        Else
            REM now it is onto the 3 classic cases of binary node deletion
            If candidatenode.LeftNode Is Nothing And candidatenode.RightNode Is Nothing Then
                REM check to see if we are dealing with the root node. If so, deletion will be pretty easy
                If candidatenode.Parent Is Nothing Then
                    Dim dummynode As PassNode = Nothing REM this variable is just used to pass into my root function
                    Form1.listofUsers(curruser).UserTree.SetUserPassTreeRoot(dummynode)
                    returnval = True
                    Form1.listofUsers(curruser).UserTree.RemoveTopBrother_TurnIntoPassNode(candidatenode)
                    returnval = True

                Else
                    REM we will have to check which side of the parent this candidatenode is on


                    If candidatenode.Parent.LeftNode Is Nothing Then
                        REM then obviously we are connected on the right side
                        candidatenode.Parent.RightNode = Nothing
                        Return True
                    ElseIf candidatenode.Parent.RightNode Is Nothing Then
                        REM then obviously we are connected on the left side
                        candidatenode.Parent.LeftNode = Nothing
                        Return True
                    End If
                    REM if we get here, then we need to do some string comps to determine which side we are on
                    Dim compresult As Boolean = String.Equals(candidatenode.username, candidatenode.Parent.LeftNode.username)
                    If compresult = True Then
                        REM obviously we're on the left side. So we unlink the parent's left node
                        candidatenode.Parent.LeftNode = Nothing
                        returnval = True

                    Else
                        REM obviously we're on the right side
                        candidatenode.Parent.RightNode = Nothing
                        returnval = True

                    End If
                End If

                REM cases with one children
            ElseIf candidatenode.LeftNode Is Nothing And Not candidatenode.RightNode Is Nothing Then
                REM figure out if we are dealing with the root node
                If candidatenode.Parent Is Nothing Then
                    REM pretty easy. Just associate the root to the candidate's right node!
                    PassTree_root = candidatenode.RightNode
                    returnval = True

                    Return returnval
                End If
                REM once again, figure out if we're on the left or right side of the parent
                If candidatenode.Parent.LeftNode Is Nothing Then
                    REM then obviously we are connected on the right side
                    candidatenode.Parent.RightNode = Nothing
                    Return True
                ElseIf candidatenode.Parent.RightNode Is Nothing Then
                    REM then obviously we are connected on the left side
                    candidatenode.Parent.LeftNode = Nothing
                    Return True
                End If

                REM if we get here, then we need to do some string comps to determine which side we are on
                Dim compresult As Boolean = String.Equals(candidatenode.username, candidatenode.Parent.LeftNode.username)
                If compresult = True Then
                    REM candidate is on left side!
                    REM so we will hook up the left arm of the parent to the right arm of the candidate node
                    candidatenode.Parent.LeftNode = candidatenode.RightNode
                    candidatenode.RightNode = Nothing
                    returnval = True

                Else
                    REM candidate is on right side!
                    candidatenode.Parent.RightNode = candidatenode.RightNode
                    returnval = True

                End If
            ElseIf Not candidatenode.LeftNode Is Nothing And candidatenode.RightNode Is Nothing Then
                REM figure out if we are dealing with the root node
                If candidatenode.Parent Is Nothing Then
                    REM pretty easy. Just associate the root to the candidate's left node!
                    PassTree_root = candidatenode.LeftNode
                    returnval = True
                    Return returnval
                End If
                REM relatively the same procedure as up top
                If candidatenode.Parent.LeftNode Is Nothing Then
                    REM then obviously we are connected on the right side
                    candidatenode.Parent.RightNode = Nothing
                    Return True
                ElseIf candidatenode.Parent.RightNode Is Nothing Then
                    REM then obviously we are connected on the left side
                    candidatenode.Parent.LeftNode = Nothing
                    Return True
                End If
                REM if we get here, then we need to do some string comps to determine which side we are on
                Dim compresult As Boolean = String.Equals(candidatenode.username, candidatenode.Parent.LeftNode.username)
                If compresult = True Then
                    REM candidate is on the left side!
                    REM we will hook up the left arm of the parent to the left arm of the candidate node
                    candidatenode.Parent.LeftNode = candidatenode.LeftNode
                    candidatenode.LeftNode = Nothing
                    returnval = True

                Else
                    REM candidate is on the right side!
                    candidatenode.Parent.RightNode = candidatenode.LeftNode
                    candidatenode.LeftNode = Nothing
                    returnval = True

                End If

                REM cases with 2 children (the trickiest)
            ElseIf Not candidatenode.LeftNode Is Nothing And Not candidatenode.RightNode Is Nothing Then

                REM this is the more difficult case
                REM we will pick the algorithm that will take the smallest value from the right subtree
                Dim minNode As PassNode = Form1.listofUsers(curruser).UserTree.FindMinPassNode_onright(candidatenode)
                REM now, the candidatenode will not be erased! We will simply replace its values
                candidatenode.password = minNode.password
                candidatenode.username = minNode.username
                REM remove the associations to minNode
                minNode.Parent = Nothing

                Dim rightTree As PassNode = minNode.RightNode
                REM this is for the case when there are still some nodes hanging on our minNode's right side
                REM our plan is to reinsert these guys back to where they belong
                minNode.LeftNode = Nothing
                minNode.RightNode = Nothing
                Form1.listofUsers(curruser).UserTree.ReinsertSubTree(rightTree)

                returnval = True
                Return returnval
            End If
        End If REM end large IF
        candidatenode.username = ""
        Array.Clear(candidatenode.password, candidatenode.password.GetLowerBound(0), candidatenode.password.Length)
        candidatenode.my_property = ""
        Return returnval
    End Function

    Public Function RemovePassNode_AllSites(ByVal sitename As String) As Boolean
        Return False
    End Function

    Private Function CreatePassNode(ByRef name As String, ByRef pass As Byte(), ByRef prop As String) As PassNode
        Dim newnode As New PassNode()
        newnode.username = name
        newnode.password = pass
        newnode.my_property = prop
        Return newnode
    End Function

    Public Function FindUsername(ByVal nametofind As String) As PassNode
        Dim tempnode As PassNode = PassTree_root
        While Not tempnode Is Nothing
            REM do some string comparisons
            Dim stringcompresult = String.Compare(nametofind, tempnode.username)
            If stringcompresult = 0 Then
                REM we have found our result!
                Return tempnode
            ElseIf stringcompresult > 0 Then
                REM we have to check the right side
                tempnode = tempnode.RightNode
                Continue While
            ElseIf stringcompresult < 0 Then
                REM we have to check the left side
                tempnode = tempnode.LeftNode
                Continue While
            Else
                REM uh... shouldn't happen but something went wrong!
                Return Nothing
            End If
        End While
        REM found nothing so return:
        Return Nothing
    End Function

    REM NOTE: in a future release of this program, this function will be updated
    REM this function goes through each PassNode that the user has and determines if the username belongs to
    REM the sitename_tofind. 
    REM RETURNS: a PassQueue with the PassNodes that have the site as my_property
    Public Function FindSiteName(ByVal sitename_tofind As String) As QueueBlock
        Dim curruser As Integer = Form2.FetchCurrentUser()
        Dim tempqueue As New PassQueue
        Dim returnblock As New QueueBlock
        tempqueue = Form1.listofUsers(curruser).UserTree.Collect()

        If tempqueue Is Nothing Then
            Return Nothing
        End If

        Dim tempblock As New QueueBlock
        While tempqueue.IsEmpty() = False
            tempblock = tempqueue.Top()
            tempqueue.Pop()
            If String.Equals(tempblock.data.my_property, sitename_tofind) = True Then
                REM then we have found our element! Attach it to our queueblock
                returnblock.InsertItem(tempblock)
            End If
            REM now check to see if there are any Brothers for this site
            If tempblock.data.Brothers.IsEmpty() = False Then
                Dim similarBrotherElements As New QueueBlock
                similarBrotherElements = tempblock.data.Brothers.Contains(sitename_tofind, 3)
                If similarBrotherElements.IsEmpty() = False Then
                    REM we actually found some Brothers!
                    REM now, we will have to add these things to our returnblock
                    Dim myiterator As New QueueBlock
                    myiterator = similarBrotherElements.Top()
                    While Not myiterator Is Nothing
                        returnblock.InsertItem(myiterator)
                        myiterator = myiterator.nextblock
                    End While
                End If
            End If
        End While

        Return returnblock
    End Function

    REM condition for this function is that root must have valid leftnode and rightnode
    REM this function also does one additional tiny thing: delete the parent's association to the child
    Private Function FindMinPassNode_onright(ByVal root As PassNode) As PassNode
        If root.RightNode Is Nothing Then
            Return Nothing
        End If

        Dim currnode As PassNode = root.RightNode
        If currnode.LeftNode Is Nothing Then
            REM this is the only case where the cleanup is a bit different
            REM we have to erase currnode.Parent.RightNode association rather than LeftNode
            root.RightNode = Nothing
            Return currnode
        End If
        While Not currnode Is Nothing
            REM now, we will always be looking at the leftnodes
            If currnode.LeftNode Is Nothing Then
                REM then currnode is the minimum node on the right!
                currnode.Parent.LeftNode = Nothing
                Return currnode
            Else
                REM continue with our trekking
                currnode = currnode.LeftNode
                Continue While
            End If
        End While
        Return Nothing
    End Function

    Public Function FetchUserPassTreeRoot() As PassNode
        Return PassTree_root
    End Function

    Public Sub SetUserPassTreeRoot(ByVal value As PassNode)
        PassTree_root = value
    End Sub

    REM this function, given a root node, reinserts the nodes into a tree
    Private Sub ReinsertSubTree(ByVal root As PassNode)
        If root Is Nothing Then
            REM there is nothing for us to insert
            Return
        End If
        Dim insertionqueue As New PassQueue
        Dim toinsert_block As New QueueBlock
        Dim curruser As Integer

        curruser = Form2.FetchCurrentUser()
        insertionqueue = Form1.listofUsers(curruser).UserTree.Collect(root)
        toinsert_block = insertionqueue.Top()
        While insertionqueue.IsEmpty() = False
            insertionqueue.Pop()
            Form1.listofUsers(curruser).UserTree.InsertPassNode(toinsert_block.data.username, toinsert_block.data.password, toinsert_block.data.my_property)
            toinsert_block = insertionqueue.Top()
        End While
    End Sub

    Public Sub RemoveTopBrother_TurnIntoPassNode(ByRef candidatenode As PassNode)
        REM we will copy the Brother data over to candidate node
        Dim tempblock As New QueueBlock
        tempblock = candidatenode.Brothers.Top()
        candidatenode.username = tempblock.data.username
        candidatenode.password = tempblock.data.password
        candidatenode.my_property = tempblock.data.my_property
        REM disconnect the Brothers' head and begin a new series
        candidatenode.Brothers.DeleteTop()
    End Sub

    Public Sub MergeBrothers(ByRef nodetocopyto As PassNode, ByRef sourcenode As PassNode)
        Dim sourcenode_block As New QueueBlock

        sourcenode_block = sourcenode.Brothers.Top()
        While Not sourcenode_block Is Nothing
            nodetocopyto.Brothers.InsertItem(sourcenode_block)
            sourcenode_block = sourcenode_block.nextblock
        End While
    End Sub

    REM this function gathers all the nodes of a PassTree and puts them into a queue
    Public Function Collect() As PassQueue
        Dim dataqueue As New PassQueue()
        Dim tempqueue As New PassQueue()
        Dim tempnode As PassNode = PassTree_root

        If tempnode Is Nothing Then
            Return Nothing
        End If

        tempqueue.InsertItem(tempnode)
        While tempqueue.IsEmpty() = False
            Dim block As New QueueBlock
            block = tempqueue.Top()
            tempqueue.Pop()
            If Not block.data.LeftNode Is Nothing Then
                tempqueue.InsertItem(block.data.LeftNode)
            End If
            If Not block.data.RightNode Is Nothing Then
                tempqueue.InsertItem(block.data.RightNode)
            End If
            dataqueue.InsertItem(block.data)

        End While
        Return dataqueue
    End Function

    REM this function gathers all the nodes of a PassTree and puts them into a queue
    Public Function Collect(ByVal root_node As PassNode) As PassQueue
        Dim dataqueue As New PassQueue()
        Dim tempqueue As New PassQueue()
        Dim tempnode As PassNode = root_node

        If tempnode Is Nothing Then
            Return Nothing
        End If

        tempqueue.InsertItem(tempnode)
        While tempqueue.IsEmpty() = False
            Dim block As New QueueBlock
            block = tempqueue.Top()
            tempqueue.Pop()
            If Not block.data.LeftNode Is Nothing Then
                tempqueue.InsertItem(block.data.LeftNode)
            End If
            If Not block.data.RightNode Is Nothing Then
                tempqueue.InsertItem(block.data.RightNode)
            End If
            dataqueue.InsertItem(block.data)

        End While
        Return dataqueue
    End Function

End Class
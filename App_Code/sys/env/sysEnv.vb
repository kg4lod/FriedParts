Imports Microsoft.VisualBasic
Imports System

'Global Variable Container
Public Module sysEnv
    'SQLServer Data Constants
    Public Const sqlFALSE As Byte = 0
    Public Const sqlTRUE As Byte = 1

    'Parts Without an Image on File
    Public Const sysNoPartPhoto As String = "~/Images/Parts/noavail.gif"

    'PART TYPES
    Public Const sysPARTTYPEUNKNOWN As Int16 = 185
    Public Const sysPARTTYPEROOT As Int16 = 0

    'SYSTEM USER
    Public Const SysUserID As Int16 = 35 'Set to the UserID of the System Service user from the [user-Accounts] table

    Public Function urlAppRoot(Optional ByVal JustServerRoot As Boolean = False) As String
        Const ServerRoot As String = "http://friedparts.nesl.ucla.edu"
        If JustServerRoot Then
            Return ServerRoot
        Else
            Return ServerRoot & "/FriedParts/"
        End If
    End Function

    '==================
    '== SERVER NAME ==
    '==================

    ''' <summary>
    ''' Is this machine the production server?
    ''' </summary>
    ''' <returns>True if the machine running this function is the server</returns>
    ''' <remarks>Used to prevent spawing of Update Service worker threads on development machines.</remarks>
    Public Function sysIsServer() As Boolean
        Select Case Environment.MachineName.ToString()
            Case "FRED" 'For debugging on FRED machine
                Return False
            Case Else
                Return True
        End Select
    End Function

    '===================
    '== PROCESS NAMES ==
    '===================

    Public Function sysWebserverProcess() As String
        Select Case Environment.MachineName.ToString()
            Case "FRED" 'For debugging on FRED machine
                Return "WebDev.WebServer40"
            Case Else
                Return "w3wp"
        End Select
    End Function

    '================
    '== FILE PATHS ==
    '================

    Public Function uploadBOMIMPORT() As String
        Select Case Environment.MachineName.ToString()
            Case "FRED" 'For debugging on FRED machine
                Return "M:\FriedPartsUploads\bomImport\"
            Case Else
                Return "C:\Inetpub\wwwroot\FriedPartsUploads\bomImport\"
        End Select
    End Function

    Public Function uploadBOMPROJECTS() As String
        Select Case Environment.MachineName.ToString()
            Case Else
                Return "C:\Inetpub\wwwroot\FriedPartsUploads\bomProjects\"
        End Select
    End Function

    Public Function sysLIBALTIUMROOT() As String
        Select Case Environment.MachineName.ToString()
            Case "FRED" 'For debugging on FRED machine
                Return "M:\FriedParts\Lib_Altium\"
            Case Else
                Return "C:\Inetpub\wwwroot\FriedParts\Lib_Altium\"
        End Select
    End Function

    ''' <summary>
    ''' The server-local path of the Dropbox storage location
    ''' </summary>
    ''' <returns>The path which is machine specific. Delimiter terminated.</returns>
    ''' <remarks>The function call enables the code to support local debugging 
    ''' environments</remarks>
    Public Function dropboxROOT() As String
        Select Case Environment.MachineName.ToString()
            Case "FRED" 'For debugging on FRED machine
                Return "M:\FriedParts\Dropbox\"
            Case Else
                Return "C:\Inetpub\wwwroot\FriedParts\Dropbox\"
        End Select
    End Function

    '===========================
    'SESSION STATE DOCUMENTATION
    '===========================
    'HTTPCONTEXT.CURRENT.SESSION
    'Session State Documentation

    '>user.Status -- Of Type LoginStates; The current status of the current session
    '>user.OpenID -- The actual OpenID identifier (for authentication)
    '>user.Name -- The actual username (from our database)
    '>user.email -- email address (typically acquired from Google)
    '>user.UserID -- The local database ID for this user!
    '>user.RoleID     -- Our DB's Permission level for this user (number)
    '>user.RoleDesc   -- Our DB's Permission level for this user (description)

    '>padd.OPRs -- OctoResults Web data structure (Digikey/Octopart)
    '>padd.PartTypeControl -- The PartType pseudo-control

    '>inv.ConflictBin -- fpInv.fpInvBinError data structure containing the bin location information for a conflict (marking a bin as empty, but it already has a part in it)
    '>inv.SearchResults -- The datasource of the returned search results (so we don't requery/reprocess)

    '>bom.Project -- The fpProject object of the project currently being entered.
    '               Also contains the datasource of the imported file analysis (so we don't requery/reprocess)
    '>bom.ProjectID -- The ProjectID to display in the buildreport

    '>dropbox.Status -- Of Type DropboxStates; The current status of the current users Dropbox account
    '>dropbox.Account -- Of Type DropboxUser; The object holding the actual data structure
    '>dropbox.Cache.Contents -- Of Type DataTable; DataSource for xGridDropboxContents in filesDropbox.aspx

End Module

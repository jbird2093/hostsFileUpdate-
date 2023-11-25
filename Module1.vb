Imports System.IO
Imports System.Security.Principal
Imports System.Text
Imports IWshRuntimeLibrary
Imports File = System.IO.File

Module Module1

    Sub Main()
        Dim ipAddy As String = "34.200.92.246"
        Dim winHostsFile As String = "C:\Windows\System32\drivers\etc\hosts"
        Dim outString As New StringBuilder
        Dim entryFound As Boolean = False
        Try
            Dim newDomain As String = GenerateRandomString(16, False) & ".com"

            Dim Principal As WindowsPrincipal = New WindowsPrincipal(WindowsIdentity.GetCurrent)
            Dim administrativeMode As Boolean = Principal.IsInRole(WindowsBuiltInRole.Administrator)

            If administrativeMode Then
                Console.WriteLine("Copying backup first.")
                If File.Exists(winHostsFile) Then
                    Dim backupFile As String = winHostsFile & Strings.Format(DatePart(DateInterval.Year, Now()), "0000") & Strings.Format(DatePart(DateInterval.Month, Now()), "00") _
                                        & Strings.Format(DatePart(DateInterval.Day, Now()), "00") & Strings.Format(DatePart(DateInterval.Hour, Now()), "00") _
                                        & Strings.Format(DatePart(DateInterval.Minute, Now()), "00") & ".bak"
                    File.Copy(winHostsFile, backupFile, True)
                    If File.Exists(backupFile) Then
                        Console.WriteLine("Backup completed.")
                    End If
                    Dim fileEntry As String = ipAddy & " " & newDomain
                    Console.WriteLine("Making alterations.")
                    For Each Line As String In File.ReadLines(winHostsFile)
                        If Line.Contains(ipAddy) = True Then
                            entryFound = True
                            outString.AppendLine(fileEntry)
                        Else
                            outString.AppendLine(Line)
                        End If
                    Next
                    If Not entryFound Then
                        For Each Line As String In File.ReadLines(winHostsFile)
                            outString.AppendLine(Line)
                        Next
                        'add the new line at the end of the file
                        outString.AppendLine(vbCrLf & fileEntry)
                    End If
                    'Console.WriteLine(outString.ToString)
                    CreateShortCut(newDomain)
                    File.WriteAllText(winHostsFile, outString.ToString())
                    Console.WriteLine("A shortcut has been created on your desktop. You will want to use this destination to reach the site.")
                    Console.WriteLine("ALL DONE! Your new domain is '" & newDomain & "' and your hosts file looks like this:" & vbCrLf)
                    Console.WriteLine(outString.ToString())
                Else
                    Console.WriteLine("hosts file does not exist. Nothing done.")
                End If
            End If
            My.Computer.Clipboard.SetText(String.Format("https://{0}", newdomain))
            Console.WriteLine("Your new domain is: " & newdomain & " and has been copied to your clipboard and you can paste into your browser.")
            Console.ReadKey()

        Catch ex As Exception
            Console.WriteLine(vbCrLf & ex.Message & vbCrLf)
            Console.ReadKey()
        End Try
    End Sub

    Public Function GenerateRandomString(ByRef len As Integer, ByRef upper As Boolean) As String
        Dim rand As New Random()
        Dim allowableChars() As Char = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLOMNOPQRSTUVWXYZ0123456789".ToCharArray()
        Dim final As String = String.Empty
        For i As Integer = 0 To len - 1
            final += allowableChars(rand.Next(allowableChars.Length - 1))
        Next

        Return IIf(upper, final.ToUpper(), final)
    End Function

    Private Sub CreateShortCut(ByVal newDomain As String)
        Try
            Dim objShell, strDesktopPath, objLink
            objShell = CreateObject("WScript.Shell")
            strDesktopPath = objShell.SpecialFolders("Desktop")
            objLink = objShell.CreateShortcut(strDesktopPath & "\" & "deepcut.lnk")
            objLink.Arguments = String.Format("https://{0}", newDomain)
            objLink.Description = "deepcut.fm"
            objLink.TargetPath = String.Format("https://{0}", newDomain)
            objLink.WindowStyle = 1
            'objLink.WorkingDirectory = ""
            objLink.Save
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Module

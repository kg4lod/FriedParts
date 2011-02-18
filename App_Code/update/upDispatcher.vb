﻿Imports Microsoft.VisualBasic
Imports System.Threading
Imports System.Data

Namespace UpdateService
    
    Public Class upDispatcher
        Inherits upProcess

        ''' <summary>
        ''' The actual semaphore object used to control access. Derivative classes MUST SHADOW this 
        ''' variable in order to dissociate from the global pool of thread resources.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Shared Shadows mutexSemaphore As Threading.Semaphore

        ''' <summary>
        ''' The timer interval in seconds. This specifies the sleep delay between 
        ''' executions of the maintainence worker (updater) dispatcher
        ''' </summary>
        ''' <remarks>Original intent was 10 seconds.</remarks>
        Private TimerInterval As Byte = 10

        ''' <summary>
        ''' The timer interval in seconds. This specifies the sleep delay between 
        ''' executions of the maintainence worker (updater) dispatcher
        ''' </summary>
        ''' <value>The interval specified in seconds (positive integers only)</value>
        ''' <returns>The current set value for the interval</returns>
        ''' <remarks>The default value is 10 seconds. Changes are not preserved in non-volatile storage. This is predominantly for debugging.</remarks>
        Public Property DispatchTimerInterval As Byte
            Get
                Return TimerInterval
            End Get
            Set(ByVal value As Byte)
                TimerInterval = value
            End Set
        End Property

        ''' <summary>
        ''' This value is used to implement a manual break which can stop the operation of the dispatcher
        ''' </summary>
        ''' <remarks>Use Start() and Stop() respectively</remarks>
        Private Shared fpusBreak As Boolean = False

        '======================================
        ' SCHEDULING / DISPATCH
        '======================================
        ''' <summary>
        ''' Control parameter which disables the use of the timing prescaler in 
        ''' the maintainence/update worker dispatcher. Without prescaling, the 
        ''' workers will be dispatched at the rate indicated by TimerInterval
        ''' </summary>
        ''' <remarks>Normal condition is intended for this to be set to False.</remarks>
        Private fpusOptionDisablePrescaler As Boolean = False

        ''' <summary>Set Now to True to disable the prescaler. This causes the dispatcher to dispatch worker threads on this execution
        ''' instead of waiting for the prescaler timer to countdown. This is extremely useful for debugging or when you need to force the 
        ''' workers to run when performing manual update management. The normal condition (and default value) is False. Now may be omitted -- 
        ''' it's optional -- and the normal case will be assumed.</summary>
        ''' <value>The new value. Set to True to disable the prescaler.</value>
        ''' <returns>The current state of the Prescaler. False = enabled.</returns>
        ''' <remarks>False is the default and normal condition.</remarks>
        Public Property DisablePrescaler As Boolean
            Get
                Return fpusOptionDisablePrescaler
            End Get
            Set(ByVal value As Boolean)
                fpusOptionDisablePrescaler = value
            End Set
        End Property

        ''' <summary>
        ''' Control parameter which disables the use of subsequent threading in the
        ''' worker dispatcher. Worker threads will execute sequentially in the same thread as 
        ''' the dispatcher. This facilities line-by-line step-through debugging.
        ''' </summary>
        ''' <remarks>Normal condition is intended for this to be set to False.</remarks>
        Private fpusOptionDisableThreading As Boolean = False

        ''' <summary>Run everything in this thread -- do not spawn worker processes. To facilitate debugging! Do not do this at runtime!</summary>
        ''' <value>Set to False to enable Threading. If True, the dispatches will run in this thread.</value>
        ''' <returns>Current state of threaded dispatch. True means no threads are forked from this one.</returns>
        Public Property DisableThreading As Boolean
            Get
                Return fpusOptionDisableThreading
            End Get
            Set(ByVal value As Boolean)
                fpusOptionDisableThreading = value
            End Set
        End Property

        ''' <summary>
        ''' This function shutsdown any dispatchers currently running at the completion 
        ''' of their current round of dispatches. It does not kill any worker threads spawned by
        ''' the dispatcher(s). These are allowed to run to completion.
        ''' </summary>
        ''' <remarks>Makes use of the internal fpusBreak variable</remarks>
        Public Shared Sub Cancel()
            fpusBreak = True
        End Sub



        ''' <summary>
        ''' The actual dispatcher thread. This is the function that gets forked.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overrides Function TheActualThread() As String
            'Threading doesn't allow the user to pass in parameters, so we handle it this way
            Dim Now As Boolean = fpusOptionDisablePrescaler
            Dim NoThreads As Boolean = fpusOptionDisableThreading


            UpdateThreadStatus(scanStatus.scanRUNNING)
            While Not fpusBreak
                'Throttle this during development
                If Now Then
                    'Debug case -- caller has requested immediate dispatch
                    fpusExecuteDispatch() 'EXECUTE! (decision to thread or not is made inside here)
                Else
                    'Normal case -- we prescale (slow) the execution rate of the dispatches
                    Const Prescaler As Byte = 6 'xxx
                    Static Dim TimerPrescaler As Byte = 0
                    TimerPrescaler = TimerPrescaler + 1
                    If TimerPrescaler = Prescaler Then
                        fpusExecuteDispatch() 'EXECUTE! (decision to thread or not is made inside here)
                        TimerPrescaler = 0 'Reset Prescaler
                    End If
                End If
                'Sleep thread!
                UpdateThreadStatus(scanStatus.scanSLEEPING)
                Thread.Sleep(1000 * TimerInterval)
            End While 'Infinite Loop!
            UpdateThreadStatus(scanStatus.scanIDLE)
            LogEvent("[OH CRAP!] Manual Dispatcher Service (Thread#: " & procMeta.GetThreadID & ") STOP Commanded By User!", logMsgTypes.msgSTOP)
            Return "[OH CRAP!] Manual Dispatcher Service STOP Commanded By User!" 'Exit from infinite loop not possible!
        End Function

        ''' <summary>
        ''' Performs the actual dispatching
        ''' </summary>
        ''' <remarks>Called exclusively by fpusDispatch() -- separates out the execution functions because they can be called under
        ''' several different conditions including with a prescaling timer (normal) and without (debug)</remarks>
        Private Sub fpusExecuteDispatch()
            'Update a part
            Dim threadPartWorker As New upWorkerPart
            'threadPartWorker.Start(Not fpusOptionDisableThreading) 'xxx Seems to be a problem in the apiOctopart

            'Update a Dropbox
            Dim threadDropboxWorker As New upWorkerDropbox
            threadDropboxWorker.Start(Not fpusOptionDisableThreading)
        End Sub

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="NoThreads">Set to True to disable thread forking. All work is done in this process.</param>
        ''' <param name="Now">Set to True to disable the prescalar. All dispatches will be issued on every DispatchTimerInterval.</param>
        ''' <remarks>Don't forget to call Start() to actually start the sync work! Just instantiating the class is not enough!</remarks>
        Public Sub New(Optional ByRef Now As Boolean = False, Optional ByRef NoThreads As Boolean = False)
            'Configure Base
            MyBase.New() 'Always do this and do it first!
            procMeta.ThreadType = upThreadTypes.ttDispatcher

            'Perform Specifics
            '[Deal with Parameters]
            fpusOptionDisablePrescaler = Now
            fpusOptionDisableThreading = NoThreads
        End Sub
    End Class
End Namespace
using System.Collections.Concurrent;
using System.Threading.Channels;
using ApiBank.Core.Interfaces;

namespace ApiBank.Infrastructure.Services;

 public class TaskManager : ITaskManager
    {
        private readonly ConcurrentDictionary<Guid, ManagedTask> _tasks = new();
        private readonly SemaphoreSlim _semaphore;
        private readonly Channel<ManagedTask> _taskChannel;
        private readonly CancellationTokenSource _cts = new();

        public TaskManager(int maxConcurrentTasks)
        {
            _semaphore = new SemaphoreSlim(maxConcurrentTasks);
            _taskChannel = Channel.CreateUnbounded<ManagedTask>(new UnboundedChannelOptions
            {
                SingleWriter = false, // Plusieurs producteurs peuvent écrire
                SingleReader = true   // Un seul consommateur lit la file
            });
            StartTaskScheduler();
        }

        public async Task<Guid> EnqueueTaskAsync(Func<CancellationToken, Task> taskFunc)
        {
            var taskId = Guid.NewGuid();
            var managedTask = new ManagedTask
            {
                TaskId = taskId,
                TaskFunc = taskFunc,
                Status = TaskStatus.WaitingToRun,
                CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token)
            };

            _tasks[taskId] = managedTask;

            // Écriture de la tâche dans le channel de manière asynchrone
            await _taskChannel.Writer.WriteAsync(managedTask);

            return taskId;
        }

        private void StartTaskScheduler()
        {
            Task.Run(async () =>
            {
                try
                {
                    await foreach (var managedTask in _taskChannel.Reader.ReadAllAsync(_cts.Token)) // Lecture asynchrone du channel
                    {
                        await _semaphore.WaitAsync(_cts.Token); 

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                managedTask.Status = TaskStatus.Running;

                                await managedTask.TaskFunc(managedTask.CancellationTokenSource.Token);

                                managedTask.Status = TaskStatus.RanToCompletion;
                            }
                            catch (OperationCanceledException)
                            {
                                managedTask.Status = TaskStatus.Canceled;
                            }
                            catch (Exception)
                            {
                                managedTask.Status = TaskStatus.Faulted;
                            }
                            finally
                            {
                                _semaphore.Release();

                                // TODO: Enregistrer le résultat ou le statut de la tâche dans la base de données ici si nécessaire
                            }
                        }, _cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Le token d'annulation a été déclenché, sortir proprement
                }
            }, _cts.Token);
        }

        public void CancelTask(Guid taskId)
        {
            if (_tasks.TryGetValue(taskId, out var managedTask))
            {
                managedTask.CancellationTokenSource.Cancel();
                managedTask.Status = TaskStatus.Canceled;
            }
        }

        public TaskStatus GetTaskStatus(Guid taskId)
        {
            if (_tasks.TryGetValue(taskId, out var managedTask))
            {
                return managedTask.Status;
            }
            throw new ArgumentException($"La tâche avec l'ID {taskId} n'a pas été trouvée.");
        }

        public void Shutdown()
        {
            _cts.Cancel();
        }

        private class ManagedTask
        {
            public Guid TaskId { get; set; }
            public Func<CancellationToken, Task> TaskFunc { get; set; }
            public TaskStatus Status { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }
    }
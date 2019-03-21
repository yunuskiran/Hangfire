﻿#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using Microsoft.Extensions.Hosting;

namespace Hangfire
{
    public class BackgroundJobServerHostedService : IHostedService, IDisposable
    {
        private readonly BackgroundJobServerOptions _options;
        private readonly JobStorage _storage;
        private readonly IEnumerable<IBackgroundProcess> _additionalProcesses;
        private readonly IBackgroundJobFactory _factory;
        private readonly IBackgroundJobPerformer _performer;
        private readonly IStateMachine _stateMachine;
        private readonly IBackgroundJobStateChanger _stateChanger;

        private IBackgroundProcessingServer _processingServer;

        public BackgroundJobServerHostedService(
            [NotNull] JobStorage storage,
            [NotNull] BackgroundJobServerOptions options,
            [NotNull] IEnumerable<IBackgroundProcess> additionalProcesses)
            : this(storage, options, additionalProcesses, null, null, null, null)
        {
        }

        public BackgroundJobServerHostedService(
            [NotNull] JobStorage storage,
            [NotNull] BackgroundJobServerOptions options,
            [NotNull] IEnumerable<IBackgroundProcess> additionalProcesses,
            [CanBeNull] IBackgroundJobFactory factory,
            [CanBeNull] IBackgroundJobPerformer performer,
            [CanBeNull] IStateMachine stateMachine,
            [CanBeNull] IBackgroundJobStateChanger stateChanger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            _additionalProcesses = additionalProcesses;

            _factory = factory;
            _performer = performer;
            _stateMachine = stateMachine;
            _stateChanger = stateChanger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _processingServer = _factory != null && _performer != null && _stateMachine != null && _stateChanger != null
                ? new BackgroundJobServer(_options, _storage, _additionalProcesses, _factory, _performer, _stateMachine, _stateChanger)
                : new BackgroundJobServer(_options, _storage, _additionalProcesses);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _processingServer?.SendStop();
            return _processingServer?.WaitForShutdownAsync(cancellationToken);
        }

        public void Dispose()
        {
            _processingServer?.Dispose();
        }
    }
}
#endif
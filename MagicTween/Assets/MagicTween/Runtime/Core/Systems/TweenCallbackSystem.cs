using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using MagicTween.Core.Components;
using MagicTween.Diagnostics;

namespace MagicTween.Core.Systems
{
    [UpdateInGroup(typeof(MagicTweenCallbackSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public sealed partial class TweenCallbackSystem : SystemBase
    {
        public bool IsExecuting => _isExecuting;

        bool _isExecuting;
        EntityQuery query;

        protected override void OnCreate()
        {
            query = SystemAPI.QueryBuilder()
                .WithAspect<TweenAspect>()
                .WithAll<TweenCallbackActions>()
                .Build();
        }

        protected override void OnUpdate()
        {
            _isExecuting = true;
            try
            {
                CompleteDependency();
                var job = new SystemJob();
                job.Run(query);
            }
            finally
            {
                _isExecuting = false;
            }
        }
        partial struct SystemJob : IJobEntity
        {
            public void Execute(TweenCallbackActions actions, in TweenCallbackFlags callbackFlags)
            {
                if ((callbackFlags.flags & CallbackFlags.OnStart) == CallbackFlags.OnStart) TryInvoke(actions.onStart);
                if ((callbackFlags.flags & CallbackFlags.OnPlay) == CallbackFlags.OnPlay) TryInvoke(actions.onPlay);
                if ((callbackFlags.flags & CallbackFlags.OnPause) == CallbackFlags.OnPause) TryInvoke(actions.onPause);
                if ((callbackFlags.flags & CallbackFlags.OnUpdate) == CallbackFlags.OnUpdate) TryInvoke(actions.onUpdate);
                if ((callbackFlags.flags & CallbackFlags.OnRewind) == CallbackFlags.OnRewind) TryInvoke(actions.onRewind);
                if ((callbackFlags.flags & CallbackFlags.OnStepComplete) == CallbackFlags.OnStepComplete) TryInvoke(actions.onStepComplete);
                if ((callbackFlags.flags & CallbackFlags.OnComplete) == CallbackFlags.OnComplete) TryInvoke(actions.onComplete);
                if ((callbackFlags.flags & CallbackFlags.OnKill) == CallbackFlags.OnKill) TryInvoke(actions.onKill);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void TryInvoke(FastAction action)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex) { Debugger.LogExceptionInsideTween(ex); }
            }
        }
    }
}
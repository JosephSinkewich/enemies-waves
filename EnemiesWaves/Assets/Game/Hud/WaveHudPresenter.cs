using System;
using VContainer.Unity;

namespace EnemiesWaves.UI
{
    public sealed class WaveHudPresenter : IInitializable, IDisposable
    {
        private readonly WaveHudModel _model;
        private readonly WaveHudView _view;

        public WaveHudPresenter(WaveHudModel model, WaveHudView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            _model.OnChanged += OnModelChanged;
            OnModelChanged(_model.State);
        }

        public void Dispose()
        {
            _model.OnChanged -= OnModelChanged;
        }

        private void OnModelChanged(WaveHudState state)
        {
            _view.UpdateView(state);
        }
    }
}

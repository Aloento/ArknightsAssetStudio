namespace SoarCraft.QYun.ArknightsAssetStudio.Activation {
    using System.Threading.Tasks;

    // For more information on understanding activation flow see
    // https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/WinUI/activation.md
    //
    // Extend this class to implement new ActivationHandlers
    public abstract class ActivationHandler<T> : IActivationHandler
        where T : class {
        // Override this method to add the activation logic in your activation handler
        protected abstract Task HandleInternalAsync(T args);

        public async Task HandleAsync(object args) => await this.HandleInternalAsync(args as T);

        public bool CanHandle(object args) =>
            // CanHandle checks the args is of type you have configured
            args is T && this.CanHandleInternal(args as T);

        // You can override this method to add extra validation on activation args
        // to determine if your ActivationHandler should handle this activation args
        protected virtual bool CanHandleInternal(T args) => true;
    }
}

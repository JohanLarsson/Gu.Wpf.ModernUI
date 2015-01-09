﻿namespace Gu.ModernUI.Windows
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// Loads XAML files using Application.LoadComponent.
    /// </summary>
    public class DefaultContentLoader : IContentLoader
    {
        /// <summary>
        /// Asynchronously loads content from specified uri.
        /// </summary>
        /// <param name="uri">The content uri.</param>
        /// <param name="cancellationToken">The token used to cancel the load content task.</param>
        /// <returns>The loaded content.</returns>
        public async Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null)
            {
                throw new InvalidOperationException("Trying to load content when dispatcher == null");
            }
            return await dispatcher.InvokeAsync(() => LoadContent(uri), DispatcherPriority.Render, cancellationToken);
        }

        /// <summary>
        /// Loads the content from specified uri.
        /// </summary>
        /// <param name="uri">The content uri</param>
        /// <returns>The loaded content.</returns>
        protected virtual object LoadContent(Uri uri)
        {
            // don't do anything in design mode
            if (ModernUIHelper.IsInDesignMode)
            {
                return null;
            }
            return Application.LoadComponent(uri);
        }
    }
}

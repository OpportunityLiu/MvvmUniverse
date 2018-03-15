using System;

namespace Opportunity.MvvmUniverse.Services
{
    /// <summary>
    /// Extension methods for <see cref="IServiceHandler{TService}"/>.
    /// </summary>
    public static class IServiceHandlerExtension
    {
        /// <summary>
        /// Get <typeparamref name="TService"/> associated with the <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler"><typeparamref name="THandler"/> to find associated <typeparamref name="TService"/>.</param>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <typeparam name="THandler">Handler type.</typeparam>
        /// <returns><typeparamref name="TService"/> associated with the <paramref name="handler"/>, or <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        public static TService GetService<TService, THandler>(this THandler handler)
            where TService : class, IService<THandler>
            where THandler : IServiceHandler<TService>
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            ServiceHandlerCollection<TService, THandler>.HandlerDic.TryGetValue(handler, out var navigator);
            return navigator;
        }
    }
}

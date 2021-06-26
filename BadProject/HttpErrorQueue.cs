using System;
using System.Collections.Generic;

namespace ImprovedAdvertisementService
{
    /// <inheritdoc />
    public class HttpErrorQueue : ErrorQueueBase
    {
        #region Private Member Variables

        /// <summary>
        /// Size below which error queue will not be cleared, regardless of timestamp
        /// </summary>
        private const int MinimumHttpErrorQueueLength = 20;

        /// <summary>
        /// Error queue - shared across all instances of this class
        /// </summary>
        private readonly static Queue<DateTime> _ErrorQueue = new Queue<DateTime>();

        /// <summary>
        /// Locking object used to ensure multiple instances do not interfere with one another, shared across all instances of this class
        /// </summary>
        private readonly static object _lockObj = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="minimumErrorQueueLength">Size below which error queue will not be cleared, regardless of timestamp</param>
        public HttpErrorQueue() : base(MinimumHttpErrorQueueLength, _ErrorQueue, _lockObj)
        { }

        #endregion
    }
}

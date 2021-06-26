using System;
using System.Collections.Generic;
using System.Linq;

namespace ImprovedAdvertisementService
{
    /// <summary>
    /// Base class for implementation of error queues of differing types
    /// </summary>
    public class ErrorQueueBase
    {
        #region Private Member Variables

        /// <summary>
        /// Error queue object
        /// </summary>
        private readonly Queue<DateTime> _ErrorQueue;

        /// <summary>
        /// Size below which error queue will not be cleared, regardless of timestamp
        /// </summary>
        private readonly int _minimumErrorQueueLength;

        /// <summary>
        /// Locking object used to ensure multiple instances do not interfere with one another
        /// </summary>
        private readonly object _lockObject;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="minimumErrorQueueLength">Size below which error queue will not be cleared, regardless of timestamp</param>
        /// <param name="errorQueue">Queue object from Subclass - set Static when all instances of Subclass should share same queue</param>
        /// <param name="lockObject">Locking object from Subclass, used to ensure multiple instances do not interfere with one another, should be static if errorQueue is</param>
        public ErrorQueueBase(int minimumErrorQueueLength, Queue<DateTime> errorQueue, object lockObject)
        {
            _minimumErrorQueueLength = minimumErrorQueueLength;
            _ErrorQueue = errorQueue;
            _lockObject = lockObject;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a timestamp to the error queue
        /// </summary>
        /// <param name="timestamp">DateTime Timestamp data to be added to error queue</param>
        public void Enqueue(DateTime timestamp)
        {
            lock(_lockObject)
                _ErrorQueue.Enqueue(timestamp);
        }

        /// <summary>
        /// Clear excess error queue items older than specified threshold. Always keep most recent <_minimumErrorQueueLength> regardless of age
        /// </summary>
        /// <param name="threshold">Timestamp threshold older than which items will be deleted</param>
        public void ClearExcessItems(DateTime threshold)
        {
            lock (_lockObject)
                while (_ErrorQueue.Count > _minimumErrorQueueLength) { 
                    if (_ErrorQueue.Peek() < threshold) _ErrorQueue.Dequeue(); 
                    else break; 
                }
        }

        /// <summary>
        /// Count queued items within specified threshold
        /// </summary>
        /// <param name="threshold">Timestamp threshold newer than which items will be counted</param>
        /// <returns>Item count</returns>
        public int CountWithinThreshold(DateTime threshold)
        {
            lock (_lockObject)
                return _ErrorQueue.ToList().Count(ts => ts > threshold);
        }

        /// <summary>
        /// Clear the error queue
        /// </summary>
        public void ClearQueue()
        {
            lock (_lockObject)
                while (_ErrorQueue.Count > 0) _ErrorQueue.Dequeue();
        }

        #endregion
    }
}

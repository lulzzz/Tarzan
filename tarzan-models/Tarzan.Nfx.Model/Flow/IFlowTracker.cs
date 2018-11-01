using System.Collections.Generic;

namespace Tarzan.Nfx.Model
{
    /// <summary>
    /// Defines operations required for flow tracker implementation.
    /// Flow key is alwas represented by <see cref="FlowKey"/> class. Flow value
    /// is given by the type parameter.
    /// </summary>
    /// <typeparam name="TFlowValue">The type of the flow record part.</typeparam>
    public interface IFlowTracker<TFlowValue>
    {
        /// <summary>
        /// Gets the dictionary representing a flow table.
        /// </summary>
        IDictionary<FlowKey, TFlowValue> FlowTable { get; }

        /// <summary>
        /// Gets the total number of frames processed so far. 
        /// </summary>
        int TotalFrameCount { get; }

        /// <summary>
        /// Process a given frame.
        /// </summary>
        /// <param name="frame"></param>
        void ProcessFrame(FrameData frame);

        /// <summary>
        /// Process all frames in the given collection.
        /// </summary>
        /// <param name="frames"></param>
        void ProcessFrames(IEnumerable<FrameData> frames);
    }
}
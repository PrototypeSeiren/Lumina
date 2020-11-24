using System;
using System.Collections.Concurrent;

namespace Lumina.Data
{
    public class FileHandleManager
    {
        private readonly Lumina _lumina;
        private readonly ConcurrentQueue< WeakReference< BaseFileHandle > > _fileQueue;

        internal FileHandleManager( Lumina lumina )
        {
            _lumina = lumina;
            _fileQueue = new ConcurrentQueue< WeakReference< BaseFileHandle > >();
        }

        /// <summary>
        /// Creates a new handle to a game file but does not load it. You will need to call <see cref="ProcessQueue"/> or the wrapper function
        /// <see cref="Lumina.ProcessFileHandleQueue"/>  yourself for these handles to be loaded, on a different thread.
        /// </summary>
        /// <param name="path">The path to the file to load</param>
        /// <typeparam name="T">The type of <see cref="FileResource"/> to load</typeparam>
        /// <returns>A handle to the file to be loaded</returns>
        public FileHandle< T > CreateHandle< T >( string path ) where T : FileResource
        {
            var handle = new FileHandle< T >( _lumina, path );
            var weakRef = new WeakReference< BaseFileHandle >( handle );
            _fileQueue.Enqueue( weakRef );

            return handle;
        }

        /// <summary>
        /// Processes enqueued file handles that haven't been loaded yet. You should call this on a different thread to process handles.
        /// </summary>
        public void ProcessQueue()
        {
            while( HasPendingFileLoads )
            {
                var res = _fileQueue.TryDequeue( out var weakRef );
                if( weakRef != null && res && weakRef.TryGetTarget( out var handle ) )
                {
                    handle.Load();
                }
            }
        }

        /// <summary>
        /// Whether the file queue contains any files that are yet to be loaded
        /// </summary>
        public bool HasPendingFileLoads => !_fileQueue.IsEmpty;

    }
}
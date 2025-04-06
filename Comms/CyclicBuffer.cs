using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Rendering;

namespace TelstarClient.Comms;

public class CyclicBuffer {

    /// <summary>
    /// This class provides a thread safe cyclic buffer. The underlying data
    /// is in the form of an array and as such is thread safe as multiple threads
    /// cannot access the same element at the same time when using Add and Remove
    /// methods.
    /// </summary>
    private char[] _buffer;

    private int _bufferSize;
    private int inPtr = 0;
    private int outtPtr = 0;
    private static object iLock = new object();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="bufferSize"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public CyclicBuffer(int bufferSize) {
        if (bufferSize < 16) {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _bufferSize = bufferSize;
        _buffer = new char[bufferSize];
    }

    /// <summary>
    /// Returns the internal buffer contents.
    /// </summary>
    public char[] InternalBuffer {
        get {
            lock (iLock) {
                return _buffer;
            }
        }
    }


    /// <summary>
    /// Adds a byte to the buffer.
    /// </summary>
    /// <param name="b"></param>
    public void Add(char c) {

        lock (iLock) {
            // we don't populate the last buffer location as this would
            // move inPtr to match outPtr and it would then appear as
            // if the buffer was empty, hence BufferSize-1.
            if (this.Count == _bufferSize - 1) {
                throw new InvalidOperationException("Buffer is full.");
            }

            _buffer[inPtr] = c;
            inPtr++;
            if (inPtr >= _bufferSize) {
                inPtr = 0;
            }
        }
    }

    /// <summary>
    /// Removes a byte from the buffer.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public char Remove() {
        
        lock (iLock) {

            if (this.Count == 0) {
                throw new Exception("Buffer is empty. Check the Count property prior to calling Remove.");
            }

            var c = _buffer[outtPtr];
            outtPtr++;
            if (outtPtr >= _bufferSize) {
                outtPtr = 0;
            }
            return c;
        }
    }

    /// <summary>
    /// Returns the number of bytes in the buffer.
    /// </summary>
    public int Count {
        get {
            lock (iLock) {
                if (outtPtr <= inPtr) {
                    return inPtr - outtPtr;
                }

                // buffer inPtr has wrapped and outPtr has not
                return _bufferSize - outtPtr + inPtr;
            }
        }
    }
}
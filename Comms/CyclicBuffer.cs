using System;
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
    private const int BufferSize = 2048;

    private readonly char[] _buffer = new char[BufferSize];
    private int inPtr = 0;
    private int outtPtr = 0;

    /// <summary>
    /// Adds a byte to the buffer.
    /// </summary>
    /// <param name="b"></param>
    public void Add(char c) {

        //lock (_buffer) {
        // we don't populate the last buffer location as this would
        // move inPtr to match outPtr and it would then appear as
        // if the buffer was empty, hence BufferSize-1.
        if (this.Count == BufferSize-1) {
            throw new InvalidOperationException("Buffer is full.");
        }
        
        _buffer[inPtr] = c;
        inPtr++;
        if (inPtr >= BufferSize) {
            inPtr = 0;
        }
        //}
    }

    /// <summary>
    /// Removes a byte from the buffer.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public char Remove() {
        char c;
        if (this.Count == 0) {
            throw new Exception("Buffer is empty. Check the Count property prior to calling Remove.");
        }

        //lock (_buffer) {
        c = _buffer[outtPtr];
        outtPtr++;
        if (outtPtr >= BufferSize) {
            outtPtr = 0;
        }
        //}

        return c;
    }

    /// <summary>
    /// Returns the number of bytes in the buffer.
    /// </summary>
    public int Count {
        get {
            if (outtPtr <= inPtr) {
                return inPtr - outtPtr;
            }

            // buffer inPtr has wrapped and outPtr has not
            return BufferSize - outtPtr + inPtr;
        }
    }
}
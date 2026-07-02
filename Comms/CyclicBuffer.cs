/*
     Copyright (c) 2025 John Newcombe
    
    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with the product. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.Threading;

namespace TelstarClient.Comms;

public class CyclicBuffer {

    /// <summary>
    /// This class provides a thread safe cyclic buffer. The underlying data
    /// is in the form of an array and as such is thread safe as multiple threads
    /// cannot access the same element at the same time when using Add and Remove
    /// methods.
    /// </summary>
    private readonly char[] _buffer;

    private readonly int _bufferSize;
    private int _inPtr;
    private int _outtPtr;
    private static readonly Lock Lock = new Lock();

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
            lock (Lock) {
                // not really needed
                return _buffer;
            }
        }
    }


    /// <summary>
    /// Adds a byte to the buffer.
    /// </summary>
    /// <param name="c"></param>
    public void Add(char c) {

        lock (Lock) {
            // we don't populate the last buffer location as this would
            // move inPtr to match outPtr and it would then appear as
            // if the buffer was empty, hence BufferSize-1.
            if (this.Count == _bufferSize - 1) {
                throw new InvalidOperationException("Buffer is full.");
            }

            _buffer[_inPtr] = c;
            _inPtr++;
            if (_inPtr >= _bufferSize) {
                _inPtr = 0;
            }
        }
    }

    /// <summary>
    /// Removes a byte from the buffer.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public char Remove() {

        lock (Lock) {

            if (this.Count == 0) {
                throw new Exception("Buffer is empty. Check the Count property prior to calling Remove.");
            }

            var c = _buffer[_outtPtr];
            _outtPtr++;
            if (_outtPtr >= _bufferSize) {
                _outtPtr = 0;
            }

            return c;
        }
    }

    /// <summary>
    /// Returns the number of bytes in the buffer.
    /// </summary>
    public int Count {
        get {
            lock (Lock) {
                if (_outtPtr <= _inPtr) {
                    return _inPtr - _outtPtr;
                }

                // buffer inPtr has wrapped and outPtr has not
                return _bufferSize - _outtPtr + _inPtr;
            }
        }
    }

    public void Clear() {
        _inPtr = 0;
        _outtPtr = 0;
    }
}
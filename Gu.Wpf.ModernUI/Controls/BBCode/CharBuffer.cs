﻿namespace Gu.Wpf.ModernUI.BBCode
{
    using System;

    /// <summary>
    /// Represents a character buffer.
    /// </summary>
    internal class CharBuffer
    {
        private readonly string value;
        private int position;
        private int mark;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharBuffer"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public CharBuffer(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.value = value;
        }

        /// <summary>
        /// Performs a look-ahead.
        /// </summary>
        /// <param name="count">The number of character to look ahead.</param>
        /// <returns></returns>
        public char LA(int count)
        {
            int index = this.position + count - 1;
            if (index < this.value.Length)
            {
                return this.value[index];
            }

            return char.MaxValue;
        }

        /// <summary>
        /// Marks the current position.
        /// </summary>
        public void Mark()
        {
            this.mark = this.position;
        }

        /// <summary>
        /// Gets the mark.
        /// </summary>
        /// <returns></returns>
        public string GetMark()
        {
            if (this.mark < this.position)
            {
                return this.value.Substring(this.mark, this.position - this.mark);
            }

            return string.Empty;
        }

        /// <summary>
        /// Consumes the next character.
        /// </summary>
        public void Consume()
        {
            this.position++;
        }
    }
}

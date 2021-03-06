﻿namespace Gu.Wpf.ModernUI.BBCode
{
    /// <summary>
    /// Represents a single token.
    /// </summary>
    internal class Token
    {
        /// <summary>
        /// Represents the token that marks the end of the input.
        /// </summary>
        public static readonly Token End = new Token(string.Empty, Lexer.TokenEnd);

        private readonly string value;
        private readonly int tokenType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="tokenType">Type of the token.</param>
        public Token(string value, int tokenType)
        {
            this.value = value;
            this.tokenType = tokenType;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value => this.value;

        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        /// <value>The type.</value>
        public int TokenType => this.tokenType;

        /// <inheritdoc />
        public override string ToString() => $"{this.tokenType}: {this.value}";
    }
}

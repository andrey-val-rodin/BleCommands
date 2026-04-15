using BleCommands.Core;
using BleCommands.Core.Events;
using System.Text;

namespace BleCommands.Tests.Core
{
    public sealed class BleStreamTests
    {
        private const char N = Constants.Terminator;

        private BleStream Stream { get; } = new();

        private List<string> Tokens { get; } = [];

        [Fact]
        public void Append_OneToken_ValidToken()
        {
            var token = "Token 1";
            Stream.TokenReceived += Stream_TokenUpdated;
            try
            {
                Stream.Append($"{token}{N}");

                Assert.Single(Tokens);
                Assert.Equal(token, Tokens[0]);
            }
            finally
            {
                Stream.TokenReceived -= Stream_TokenUpdated;
            }
        }

        [Fact]
        public void Append_UnicodeToken_ValidToken()
        {
            var token = "Русский токен🔥";
            Stream.TokenReceived += Stream_TokenUpdated;
            try
            {
                Stream.Append($"{token}{N}");

                Assert.Single(Tokens);
                Assert.Equal(token, Tokens[0]);
            }
            finally
            {
                Stream.TokenReceived -= Stream_TokenUpdated;
            }
        }

        [Fact]
        public void Append_FirstEmptyToken_ValidTokens()
        {
            var token1 = "";
            var token2 = "Token 2";
            var token3 = "Token 3";
            Stream.TokenReceived += Stream_TokenUpdated;
            try
            {
                Stream.Append($"{token1}{N}");
                Stream.Append($"{token2}{N}");
                Stream.Append($"{token3}{N}");

                Assert.Equal(3, Tokens.Count);
                Assert.Equal(token1, Tokens[0]);
                Assert.Equal(token2, Tokens[1]);
                Assert.Equal(token3, Tokens[2]);
            }
            finally
            {
                Stream.TokenReceived -= Stream_TokenUpdated;
            }
        }

        [Fact]
        public void Append_LastEmptyToken_ValidTokens()
        {
            var token1 = "Token 1";
            var token2 = "Token 2";
            var token3 = "";
            Stream.TokenReceived += Stream_TokenUpdated;
            try
            {
                Stream.Append($"{token1}{N}");
                Stream.Append($"{token2}{N}");
                Stream.Append($"{token3}{N}");

                Assert.Equal(3, Tokens.Count);
                Assert.Equal(token1, Tokens[0]);
                Assert.Equal(token2, Tokens[1]);
                Assert.Equal(token3, Tokens[2]);
            }
            finally
            {
                Stream.TokenReceived -= Stream_TokenUpdated;
            }
        }


        [Fact]
        public void Append_EmptyTokenInTheMiddle_ValidTokens()
        {
            var token1 = "Token 1";
            var token2 = "";
            var token3 = "Token 3";
            Stream.TokenReceived += Stream_TokenUpdated;
            try
            {
                Stream.Append($"{token1}{N}");
                Stream.Append($"{token2}{N}");
                Stream.Append($"{token3}{N}");

                Assert.Equal(3, Tokens.Count);
                Assert.Equal(token1, Tokens[0]);
                Assert.Equal(token2, Tokens[1]);
                Assert.Equal(token3, Tokens[2]);
            }
            finally
            {
                Stream.TokenReceived -= Stream_TokenUpdated;
            }
        }

        [Fact]
        public void Append_DisruptedChain_ValidTokens()
        {
            var chain1 = $"Token 1{N}Token 2";
            var chain2 = $"{N}Token 3{N}";
            Stream.TokenReceived += Stream_TokenUpdated;
            try
            {
                Stream.Append(chain1);
                Stream.Append(chain2);

                Assert.Equal(3, Tokens.Count);
                Assert.Equal("Token 1", Tokens[0]);
                Assert.Equal("Token 2", Tokens[1]);
                Assert.Equal("Token 3", Tokens[2]);
            }
            finally
            {
                Stream.TokenReceived -= Stream_TokenUpdated;
            }
        }

        [Fact]
        public void Append_LongDisruptedChain_ValidTokens()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                builder.Append($"Token {i}{N}");
            }
            string str = builder.ToString();
            Stream.TokenReceived += Stream_TokenUpdated;
            try
            {
                // Append big string by characters
                for (int i = 0; i < str.Length; i++)
                {
                    Stream.Append(str[i].ToString());
                }

                Assert.Equal(100, Tokens.Count);
                for (int i = 0; i < Tokens.Count; i++)
                {
                    Assert.Equal($"Token {i}", Tokens[i]);
                }
            }
            finally
            {
                Stream.TokenReceived -= Stream_TokenUpdated;
            }
        }

        private void Stream_TokenUpdated(object? sender, TextEventArgs e)
        {
            Tokens.Add(e.Value);
        }
    }
}

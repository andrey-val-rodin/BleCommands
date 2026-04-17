using BleCommands.Core;
using BleCommands.Core.Events;
using BleCommands.Core.Exceptions;
using System.Text;

namespace BleCommands.Tests.Core
{
    public sealed class TokenAggregatorTests
    {
        private const char D = TokenAggregator.DefaultTokenDelimiter;

        private TokenAggregator Aggregator { get; } = new();

        private List<string> Tokens { get; } = [];

        [Fact]
        public void Append_Null_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                Aggregator.Append(null!);
            });
            Assert.Equal("text", exception.ParamName);
        }

        [Fact]
        public void Append_OneToken_ValidToken()
        {
            var token = "Token 1";
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                Aggregator.Append($"{token}{D}");

                Assert.Single(Tokens);
                Assert.Equal(token, Tokens[0]);
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }

        [Fact]
        public void Append_UnicodeToken_ValidToken()
        {
            var token = "Русский токен🔥";
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                Aggregator.Append($"{token}{D}");

                Assert.Single(Tokens);
                Assert.Equal(token, Tokens[0]);
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }

        [Fact]
        public void Append_FirstEmptyToken_ValidTokens()
        {
            var token1 = "";
            var token2 = "Token 2";
            var token3 = "Token 3";
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                Aggregator.Append($"{token1}{D}");
                Aggregator.Append($"{token2}{D}");
                Aggregator.Append($"{token3}{D}");

                Assert.Equal(3, Tokens.Count);
                Assert.Equal(token1, Tokens[0]);
                Assert.Equal(token2, Tokens[1]);
                Assert.Equal(token3, Tokens[2]);
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }

        [Fact]
        public void Append_LastEmptyToken_ValidTokens()
        {
            var token1 = "Token 1";
            var token2 = "Token 2";
            var token3 = "";
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                Aggregator.Append($"{token1}{D}");
                Aggregator.Append($"{token2}{D}");
                Aggregator.Append($"{token3}{D}");

                Assert.Equal(3, Tokens.Count);
                Assert.Equal(token1, Tokens[0]);
                Assert.Equal(token2, Tokens[1]);
                Assert.Equal(token3, Tokens[2]);
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }


        [Fact]
        public void Append_EmptyTokenInTheMiddle_ValidTokens()
        {
            var token1 = "Token 1";
            var token2 = "";
            var token3 = "Token 3";
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                Aggregator.Append($"{token1}{D}");
                Aggregator.Append($"{token2}{D}");
                Aggregator.Append($"{token3}{D}");

                Assert.Equal(3, Tokens.Count);
                Assert.Equal(token1, Tokens[0]);
                Assert.Equal(token2, Tokens[1]);
                Assert.Equal(token3, Tokens[2]);
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }

        [Fact]
        public void Append_ThreeEmptyTokens_ValidTokens()
        {
            var token1 = "";
            var token2 = "";
            var token3 = "";
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                Aggregator.Append($"{token1}{D}");
                Aggregator.Append($"{token2}{D}");
                Aggregator.Append($"{token3}{D}");

                Assert.Equal(3, Tokens.Count);
                Assert.Equal(token1, Tokens[0]);
                Assert.Equal(token2, Tokens[1]);
                Assert.Equal(token3, Tokens[2]);
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }

        [Fact]
        public void Append_DisruptedChain_ValidTokens()
        {
            var chain1 = $"Token 1{D}Token 2";
            var chain2 = $"{D}Token 3{D}";
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                Aggregator.Append(chain1);
                Aggregator.Append(chain2);

                Assert.Equal(3, Tokens.Count);
                Assert.Equal("Token 1", Tokens[0]);
                Assert.Equal("Token 2", Tokens[1]);
                Assert.Equal("Token 3", Tokens[2]);
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }

        [Fact]
        public void Append_LongDisruptedChain_ValidTokens()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                builder.Append($"Token {i}{D}");
            }
            string str = builder.ToString();
            Aggregator.TokenReceived += Aggregator_TokenReceived;
            try
            {
                // Append big string by characters
                for (int i = 0; i < str.Length; i++)
                {
                    Aggregator.Append(str[i].ToString());
                }

                Assert.Equal(100, Tokens.Count);
                for (int i = 0; i < Tokens.Count; i++)
                {
                    Assert.Equal($"Token {i}", Tokens[i]);
                }
            }
            finally
            {
                Aggregator.TokenReceived -= Aggregator_TokenReceived;
            }
        }

        private void Aggregator_TokenReceived(object? sender, TextEventArgs e)
        {
            Tokens.Add(e.Value);
        }
    }
}

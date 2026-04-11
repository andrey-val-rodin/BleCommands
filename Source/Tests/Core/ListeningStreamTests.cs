using Core;
using Core.Contracts;
using System.Text;

namespace Tests.Core
{
    public sealed class ListeningStreamTests : IDisposable
    {
        private const char N = Constants.Terminator;
        private readonly ListeningStream _stream = new();

        [Fact]
        public void Append_OneToken_ValidToken()
        {
            var token = "Token 1";
            var tokens = new List<string>();

            _stream.Append($"{token}{N}", (a, e) =>
            {
                tokens.Add(e.Text);
            });

            Assert.Single(tokens);
            Assert.Equal(token, tokens[0]);
            Assert.Equal(0, _stream.Length);
        }

        [Fact]
        public void Append_TwoTokens_ValidTokens()
        {
            var token1 = "Token 1";
            var token2 = "Token 2";
            var tokens = new List<string>();

            _stream.Append($"{token1}{N}{token2}{N}", (a, e) =>
            {
                tokens.Add(e.Text);
            });

            Assert.Equal(2, tokens.Count);
            Assert.Equal(token1, tokens[0]);
            Assert.Equal(token2, tokens[1]);
            Assert.Equal(0, _stream.Length);
        }

        [Fact]
        public void Append_DisruptedChain_ValidTokens()
        {
            var chain1 = $"Token 1{N}Token 2";
            var chain2 = $"{N}Token 3{N}";
            var tokens = new List<string>();
            _stream.Append(chain1, (a, e) =>
            {
                tokens.Add(e.Text);
            });
            _stream.Append(chain2, (a, e) =>
            {
                tokens.Add(e.Text);
            });

            Assert.Equal(3, tokens.Count);
            Assert.Equal("Token 1", tokens[0]);
            Assert.Equal("Token 2", tokens[1]);
            Assert.Equal("Token 3", tokens[2]);
            Assert.Equal(0, _stream.Length);
        }

        [Fact]
        public void Append_LongDisruptedChain_ValidTokens()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                builder.Append($"Token {i}{N}");
            }

            var tokens = new List<string>();
            string str = builder.ToString();
            int current = 0;
            var stream = new ListeningStream();
            stream.TokenUpdated += (a, e) =>
            {
                tokens.Add(e.Text);
            };

            while (current < str.Length)
            {
                var count = Math.Min(10, str.Length - current - 1);
                var substring = str.Substring(current, count);
                stream.Append(substring);
                current += 10;
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.Equal($"Token {i}", tokens[i]);
            }
            Assert.Equal(0, _stream.Length);
        }

        [Fact]
        public void Append_BigSequence_ValidTokens()
        {
            int current = 1;
            for (int i = 0; i < 1000; i++)
            {
                var token = $"Token {current++}";
                _stream.Append(token + N, (a, e) =>
                {
                    Assert.Equal(token, e.Text);
                });
            }

            Assert.Equal(0, _stream.Length);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}

using FluentAssertions;
using NSubstitute;
using SimpleGPT.Core.Clients;

namespace SimpleGPT.Core.Tests
{
    public class GptEvaluatorTests
    {
        private readonly IGptClient _gptClientMock;
        private readonly string     _question;
        private readonly string     _expectedResult;
        
        private GptEvaluator _gptEvaluator;


        public GptEvaluatorTests()
        {
            _question = "my question";
            _expectedResult = "gpt eval result";

            _gptClientMock = Substitute.For<IGptClient>();
            _gptClientMock.CallGpt(Arg.Any<string>(), Arg.Any<GptCallOptions>()).Returns(_expectedResult);

            _gptEvaluator = new GptEvaluator(_gptClientMock, "", "");
        }

        [Fact]
        public async Task WhenEvaluate_ShouldUseProperQuestion()
        {
            // act
            var result = await _gptEvaluator.Evaluate(_question);

            // assert
            result.Should().Be(_expectedResult);
            await _gptClientMock.Received().CallGpt(Arg.Is<string>(a => a.Contains(_question)), Arg.Any<GptCallOptions>());
        }

        [Theory]
        [InlineData("my-instruct", "", "")]
        [InlineData("my-instruct", "my-knowledge", "")]
        [InlineData("my-instruct", "my-knowledge", "my-context")]
        public async Task WhenEvaluate_GivenCertainCtorParams_ShouldUseProperValues(string instruction, string knowledge, string context)
        {
            // arrange
            _gptEvaluator = new GptEvaluator(_gptClientMock, instruction, knowledge, context);

            // act
            var result = await _gptEvaluator.Evaluate(_question);

            // assert
            result.Should().Be(_expectedResult);
            await _gptClientMock.Received().CallGpt(Arg.Is<string>(a => a.Contains(instruction)), Arg.Any<GptCallOptions>());
            await _gptClientMock.Received().CallGpt(Arg.Is<string>(a => a.Contains(knowledge)), Arg.Any<GptCallOptions>());
            await _gptClientMock.Received().CallGpt(Arg.Is<string>(a => a.Contains(context)), Arg.Any<GptCallOptions>());
        }
    }
}
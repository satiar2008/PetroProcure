using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Indents;

public sealed record IndentNumberParts(
    string IndentNumber,
    int YearPart,
    int TypeDigit,
    int Sequence,
    IndentType IndentType);

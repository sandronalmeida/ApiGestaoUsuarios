using System.Text.Json;

namespace ApiGestaoUsuarios.Infraestructure.Serialization
{
    /// <summary>
    /// Converte propriedades para snake_case
    /// </summary>
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            // Converte PascalCase ou camelCase para snake_case
            return string.Concat(
                System.Text.RegularExpressions.Regex
                    .Replace(name, "([a-z0-9])([A-Z])", "$1_$2"))
                .ToLower();
        }
    }
}

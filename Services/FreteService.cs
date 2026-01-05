using SiteLoja.Interface;

namespace SiteLoja.Services
{
    // Serviço mock que simula o cálculo de frete por região.
    // Em produção, isso seria substituído por uma chamada de API (ex: Correios, Jadlog).
    public class FreteService : IFreteService
    {
        // Mock de cálculo de frete baseado no CEP (prefixo de 2 dígitos).
        public async Task<decimal> CalcularFrete(string cep, decimal pesoProdutoKg = 0.5m)
        {
            await Task.Delay(50); // Simula um delay de rede

            if (string.IsNullOrWhiteSpace(cep))
            {
                // Frete padrão se o CEP for desconhecido
                return 35.00m;
            }

            // Normaliza o CEP (remove traços e espaços)
            string cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

            if (cepLimpo.Length < 8)
            {
                // Frete padrão se o CEP for incompleto
                return 35.00m;
            }

            // Lógica de cálculo mock por região (baseada no prefixo)
            string prefixo = cepLimpo.Substring(0, 2);

            // Região Sudeste (0x, 1x, 2x, 3x)
            if (prefixo.StartsWith("0") || prefixo.StartsWith("1") || prefixo.StartsWith("2") || prefixo.StartsWith("3"))
            {
                return 19.90m;
            }
            // Região Sul (8x, 9x)
            else if (prefixo.StartsWith("8") || prefixo.StartsWith("9"))
            {
                return 24.90m;
            }
            // Região Nordeste (4x, 5x)
            else if (prefixo.StartsWith("4") || prefixo.StartsWith("5"))
            {
                return 39.90m;
            }
            // Outras regiões (Norte/Centro-Oeste e Padrão)
            else
            {
                return 29.90m;
            }
        }
    }
}
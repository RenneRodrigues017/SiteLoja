namespace SiteLoja.Interface
{
    // Define o contrato para serviços de cálculo de frete.
    public interface IFreteService
    {
        // Retorna o valor do frete em Reais (decimal) baseado no CEP e peso (peso é mockado para simplicidade).
        Task<decimal> CalcularFrete(string cep, decimal pesoProdutoKg = 0.5m);
    }
}
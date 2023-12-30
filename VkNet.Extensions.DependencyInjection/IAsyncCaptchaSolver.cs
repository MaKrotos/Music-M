namespace VkNet.Extensions.DependencyInjection;

/// <summary>
/// Определяет интерфейс взаимодействия с сервисом-распознавателем капчи
/// </summary>
public interface IAsyncCaptchaSolver
{
    /// <summary>Распознает текст капчи.</summary>
    /// <param name="url"> Ссылка на изображение капчи. </param>
    /// <returns> Строка, содержащая текст, который был закодирован в капче. </returns>
    ValueTask<string?> SolveAsync(string url);

    /// <summary>
    /// Сообщает сервису, что последняя капча была распознана неверно.
    /// </summary>
    ValueTask SolveFailedAsync();
}
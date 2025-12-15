namespace BlApi;

//==================== BL Factory ===================\\

/// <summary>
/// Factory class for accessing the Business Logic Layer.
/// Provides a centralized entry point to obtain the implementation of IBl,
/// decoupling the presentation layer from the specific BL implementation.
/// </summary>
public static class Factory
{
    /// <summary>
    /// Creates and returns the main Business Logic implementation instance.
    /// </summary>
    /// <returns>An instance implementing the <see cref="IBl"/> interface.</returns>
    public static IBl Get() => new BlImplementation.Bl();
}
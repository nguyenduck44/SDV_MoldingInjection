using EQX.UI.Language;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddLanguageServiceExtension
    {
        public static IHostBuilder AddLanguageService(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ILanguageService>(s => {
                    IEnumerable<ILanguageDefinition> supportedLanguages = new List<ILanguageDefinition>
                    {
                        new VietnameseLanguage($"/SDV_MoldingInjection;component/Resource/Language/VietnameseLanguage.xaml"),
                        new EnglishLanguage($"/SDV_MoldingInjection;component/Resource/Language/EnglishLanguage.xaml"),
                        new KoreanLanguage($"/SDV_MoldingInjection;component/Resource/Language/KoreanLanguage.xaml")
                    };

                    return new LanguageService(supportedLanguages);
                });
            });

            return hostBuilder;
        }
    }
}

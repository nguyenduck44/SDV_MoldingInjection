using EQX.UI.Language;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SDV_BIM_Line_DotDispenser.Extensions
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
                        new VietnameseLanguage($"/SDV_BIM_Line_DotDispenser;component/Resource/Language/VietnameseLanguage.xaml"),
                        new EnglishLanguage($"/SDV_BIM_Line_DotDispenser;component/Resource/Language/EnglishLanguage.xaml"),
                        new KoreanLanguage($"/SDV_BIM_Line_DotDispenser;component/Resource/Language/KoreanLanguage.xaml")
                    };

                    return new LanguageService(supportedLanguages);
                });
            });

            return hostBuilder;
        }
    }
}

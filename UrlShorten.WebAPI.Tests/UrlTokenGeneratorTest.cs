using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using UrlShorten.WebAPI.UrlTokenGenerator;

namespace UrlShorten.WebAPI.Tests
{
    public class UrlTokenGeneratorTest
    {
        [Fact]
        public void GetUrlToken_Should_Retrun_Valid_Url_Token_For_Original_Url()
        {
            using (IUrlTokenGenerator urlShortener = new Md5UrlTokenGenerator())
            {
                var originalUrls = new List<string>() 
                {
                    "book.douban.com/subject/4924165",
                    "www.manongbook.com/other/929.html",
                    "www.zhihu.com/question/393498959",
                    "www.cnblogs.com/qingyunye/p/13978903.html",
                    "blog.csdn.net/BillCYJ/article/details/89742049",
                    "zhidao.baidu.com/question/1511533557385902260.html",
                    "www.programminghunter.com/article/6548948008",
                    "zhidao.baidu.com/question/1386287829094902380.html",
                    "www.amazon.com/CLR-via-4th-Developer-Reference/dp/0735667454",
                    "www.microsoftpressstore.com/store/clr-via-c-sharp-9780735667457",
                    "bbs.csdn.net/topics/390362532",
                    "zhuanlan.zhihu.com/p/394210703",
                    "www.geeksforgeeks.org/common-language-runtime-clr-in-c-sharp",
                    "www.douban.com/subject/1919818",
                    "zhuanlan.zhihu.com/p/385504917",
                    "book.douban.com/subject/26285940",
                    "blog.csdn.net/u013553804/article/details/85100713",
                    "zhidao.baidu.com/question/688939197220952084.html"
                };

                var shortenUrls = new List<string>();

                originalUrls.ForEach(url => shortenUrls.Add(urlShortener.GetUrlToken(url)));

                Assert.DoesNotContain<string>(shortenUrls, url => url.Length != 7);
                Assert.Equal<int>(shortenUrls.Distinct().Count(), shortenUrls.Count());
                Assert.DoesNotContain<string>(shortenUrls, url => url.Any(c => !Char.IsLetterOrDigit(c)));
            }
        }
    }
}

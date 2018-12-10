using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace CoreTest
{
    /// <summary>
    /// 启动类
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;

            //var builder = new ConfigurationBuilder()
            //   .SetBasePath(env.ContentRootPath)
            //   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            //this.Configuration = builder.Build();

            //BaseConfigModel.SetBaseConfig(Configuration);
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }


        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// 第一次请求时配置各个实例对象（bean）
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(o =>
            {
                o.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<MvcOptions>(options =>
            {
                //给全局路由添加统一前缀
                options.Conventions.Insert(0, new RouteConvention(new RouteAttribute("services/v1/")));
            });

            #region swagger
            services.AddSwaggerGen(c =>
            {
                //文档左上角的描述
                var swaggerInfo = new Info
                {
                    Version = "v1.0.0",
                    Title = "hepeng's dotnetcore test",
                    Description = "路漫漫其修远兮 吾将上下而求索<br />愿你出走半生 归来仍是少年",
                    TermsOfService = "http://www.baidu.com",
                    License = new License() { Name = "license", Url = "http://www.baidu.com" },
                    Contact = new Contact() { Name = "hepeng", Email = "914535402@qq.com", Url = "https://www.cnblogs.com/hepeng/" }
                };
                c.SwaggerDoc("v1", swaggerInfo);
                //读取注释用于显示
                c.IncludeXmlComments(AppDomain.CurrentDomain.BaseDirectory + "CoreTest.xml", true);

                //在swagger中显示JWT信息
                var security = new Dictionary<string, IEnumerable<string>> { { "Bearer", new string[] { } } };
                c.AddSecurityRequirement(security);//添加一个必须的全局安全信息，和AddSecurityDefinition方法指定的方案名称要一致，这里是Bearer。
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 参数结构: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = "header",//jwt默认存放Authorization信息的位置(请求头中)
                    Type = "apiKey"
                });

            });
            #endregion

            #region 认证
            //bearer “持票人”约定俗成
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                JwtAuthConfigModel jwtConfig = new JwtAuthConfigModel();
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "CoreTest",//发行人
                    ValidAudience = "wr",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.JWTSecretKey)),

                    /***********************************TokenValidationParameters的参数默认值***********************************/
                    RequireSignedTokens = true,
                    // SaveSigninToken = false,
                    // ValidateActor = false,
                    // 将下面两个参数设置为false，可以不验证Issuer和Audience，但是不建议这样做。
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    // 是否要求Token的Claims中必须包含 Expires
                    RequireExpirationTime = true,
                    // 允许的服务器时间偏移量
                    // ClockSkew = TimeSpan.FromSeconds(300),
                    // 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    ValidateLifetime = true
                };
            });
            #endregion

            #region 授权
            services.AddAuthorization(options =>
            {
                //此处与控制器中的[Authorize(Roles = "Admin,hepeng")]对应
                //可通过读取数据角色动态添加
                options.AddPolicy("RequireClient", policy => policy.RequireRole("Client").Build());
                options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin").Build());
                options.AddPolicy("RequireAdminOrClient", policy => policy.RequireRole("Admin,Client").Build());
            });
            #endregion

            #region CORS 启用跨域请求
            //同源三要素: 协议 域名 端口  不同的资源的这三个要素同时相同才叫同源
            //https://i.cnblogs.com/EditLinks.aspx?catid=1357952
            services.AddCors(c =>
            {
                //添加策略
                //此处与控制器中的[EnableCors("Any")]对应
                c.AddPolicy("Any", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                });

                c.AddPolicy("Limit", policy =>
                {
                    policy
                    .WithOrigins("localhost:8083")
                    .WithMethods("get", "post", "put", "delete")
                    //.WithHeaders("Authorization");
                    .AllowAnyHeader();
                });
            });
            #endregion
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //认证
            app.UseAuthentication();
            //授权
            app.UseMiddleware<JwtAuthorizationFilter>();
            app.UseMvc();
            app.UseStaticFiles();

            #region swagger
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "api文档 v1");
            });
            #endregion

            //JWT规则 由三部分组成：Header（头部）、Payload（数据）、Signature（签名），将这三部分由‘.’连接而组成字符串然后加密就成为JWT字符串
            //Header:由且只由两个数据组成，一个是“alg”（加密规范）指定了该JWT字符串的加密规则，另一个是“typ”(JWT字符串类型) 列如：{"alg": "HS256","typ": "JWT"}
            //Payload（数据）:由一组数据组成，它负责传递数据，一般是发起请求的用户的信息
            //Signature（签名）：由4个因素所同时决定：编码后的header字符串，编码后的payload字符串，之前在头部声明的加密算法，我们自定义的一个秘钥字符串（secret）
            //列如：HMACSHA256( base64UrlEncode(header) + "." + base64UrlEncode(payload), secret)

            //“令牌”指的是用于http传输headers中用于验证授权的JSON数据，它是key和value两部分组成
            //key为“Authorization”，value为“Bearer {JWT字符串}”，其中value除了JWT字符串外，还在前面添加了“Bearer ”字符串，这里可以把它理解为大家约定俗成的规定即可，没有实际的作用


            //即使是初始项目 启动时也会有private.corelib.dll的异常 
        }
    }
}

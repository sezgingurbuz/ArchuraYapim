const path = require('path');

module.exports = {
  // Canlıya alırken 'production', geliştirirken 'development' yapabilirsin
  mode: 'production',

  // React uygulamasının giriş noktası
  entry: './src/index.js',

  // Çıktı ayarları: Derlenmiş dosyayı .NET projesinin wwwroot/js klasörüne gönderir
  output: {
    path: path.resolve(__dirname, '../wwwroot/js'),
    filename: 'bundle.js',
    publicPath: '/js/'
  },

  module: {
    rules: [
      {
        // .js ve .jsx uzantılı dosyaları bul
        test: /\.(js|jsx)$/,
        exclude: /node_modules/, // node_modules klasörünü dahil etme
        use: {
          loader: 'babel-loader',
          options: {
            // .babelrc dosyası aramak yerine ayarları buraya gömüyoruz
            presets: [
              ['@babel/preset-env', { targets: "defaults" }],
              ['@babel/preset-react', { runtime: "automatic" }] // React 17+ için automatic JSX
            ]
          }
        }
      },
      // CSS dosyaları için loader (Eğer CSS import edersen gerekir)
      // Bunu kullanmak için: npm install --save-dev style-loader css-loader
      {
        test: /\.css$/i,
        use: [
          'style-loader',
          'css-loader',
          'postcss-loader' // Add this
        ],
      },
    ]
  },

  // Import ederken .js veya .jsx yazmasan da anlamasını sağlar
  resolve: {
    extensions: ['.js', '.jsx']
  }
};
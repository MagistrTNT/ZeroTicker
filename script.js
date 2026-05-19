(function () {
  'use strict';

  var DEFAULTS = {
    position: 'top',
    speed: 60,
    height: 40,
    fontSize: 16,
    color: '#ffd700',
    background: 'rgba(0,0,0,0.88)'
  };

  var REFRESH_MS = 60000;

  var ticker = document.getElementById('ticker');
  if (!ticker) return;

  var roller = document.createElement('div');
  roller.id = 'ticker-roller';
  ticker.appendChild(roller);

  var spanA = document.createElement('span');
  var spanB = document.createElement('span');
  spanA.style.display = 'inline-block';
  spanB.style.display = 'inline-block';
  roller.appendChild(spanA);
  roller.appendChild(spanB);

  var pending = null;
  var config = {};

  var dynamicStyles = document.createElement('style');
  dynamicStyles.id = 'ticker-dynamic';
  document.head.appendChild(dynamicStyles);

  function applyConfig(cfg) {
    config = cfg;
    ticker.style.top = cfg.position === 'top' ? '0' : 'auto';
    ticker.style.bottom = cfg.position === 'top' ? 'auto' : '0';
    ticker.style.height = cfg.height + 'px';
    ticker.style.lineHeight = cfg.height + 'px';
    ticker.style.fontSize = cfg.fontSize + 'px';
    ticker.style.color = cfg.color;
    ticker.style.background = cfg.background;
    dynamicStyles.textContent =
      '#ticker::before { background: linear-gradient(90deg, ' + cfg.background + ', transparent); }' +
      '#ticker::after { background: linear-gradient(270deg, ' + cfg.background + ', transparent); }';
  }

  roller.addEventListener('animationiteration', function () {
    if (pending !== null) {
      var t = pending;
      pending = null;
      applyText(t.text, t.config);
    }
  });

  function applyText(text, cfg) {
    applyConfig(cfg || config || DEFAULTS);
    spanA.textContent = text;
    spanB.textContent = text;
    var w = Math.round(spanA.getBoundingClientRect().width);
    if (w > 0) {
      var speed = cfg ? cfg.speed : config.speed || DEFAULTS.speed;
      roller.style.animation = 'ticker-scroll ' + (w / speed) + 's linear infinite';
    }
  }

  function loadData(cb) {
    var prev = document.querySelector('script[data-ticker]');
    if (prev) prev.remove();

    var s = document.createElement('script');
    s.setAttribute('data-ticker', '');
    s.src = 'data.js?_=' + Date.now();
    s.onload = function () {
      if (window.rssData) cb(window.rssData);
    };
    document.body.appendChild(s);
  }

  loadData(function (d) {
    var cfg = d.config || DEFAULTS;
    if (d.text) applyText(d.text, cfg);
  });

  setInterval(function () {
    loadData(function (d) {
      var cfg = d.config || DEFAULTS;
      if (d.text && d.text !== spanA.textContent) {
        pending = { text: d.text, config: cfg };
      } else if (cfg !== config) {
        applyConfig(cfg);
      }
    });
  }, REFRESH_MS);
})();

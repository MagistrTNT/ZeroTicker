(function () {
  'use strict';

  var DEFAULTS = {
    position: 'top',
    speed: 120,
    fontSize: 30,
    fontFamily: "Verdana, 'Segoe UI', system-ui, sans-serif",
    fontWeight: 700,
    letterSpacing: 1,
    textTransform: 'uppercase',
    color: '#e0f7fa',
    background: 'rgba(0,0,0,0.5)',
    gradientWidth: 60
  };

  var REFRESH_MS = 60000;

  var ticker = document.getElementById('ticker');
  if (!ticker) return;

  var roller = document.createElement('div');
  roller.id = 'ticker-roller';
  roller.style.cssText = 'display:inline-block;white-space:nowrap;will-change:transform;';
  ticker.appendChild(roller);

  var spanA = document.createElement('span');
  var spanB = document.createElement('span');
  spanA.style.display = 'inline-block';
  spanB.style.display = 'inline-block';
  roller.appendChild(spanA);
  roller.appendChild(spanB);

  var dynamicStyles = document.createElement('style');
  dynamicStyles.id = 'ticker-dynamic';
  document.head.appendChild(dynamicStyles);

  var animId = null;
  var scrollPos = 0;
  var textWidth = 0;
  var pxPerSec = DEFAULTS.speed;
  var lastTime = 0;
  var currentCfg = {};

  function applyConfig(cfg) {
    currentCfg = cfg;
    pxPerSec = cfg.speed || DEFAULTS.speed;
    ticker.style.top = cfg.position === 'top' ? '0' : 'auto';
    ticker.style.bottom = cfg.position === 'top' ? 'auto' : '0';
    ticker.style.height = 'auto';
    ticker.style.padding = Math.max(4, Math.round(cfg.fontSize / 6)) + 'px 0';
    ticker.style.lineHeight = '1.2';
    ticker.style.fontSize = cfg.fontSize + 'px';
    ticker.style.fontFamily = cfg.fontFamily;
    ticker.style.fontWeight = cfg.fontWeight;
    ticker.style.letterSpacing = cfg.letterSpacing + 'px';
    ticker.style.textTransform = cfg.textTransform;
    ticker.style.color = cfg.color;
    ticker.style.background = cfg.background;
    var gw = cfg.gradientWidth || 60;
    dynamicStyles.textContent =
      '#ticker::before, #ticker::after { width: ' + gw + 'px; }' +
      '#ticker::before { background: linear-gradient(90deg, ' + cfg.background + ', transparent); }' +
      '#ticker::after { right: 0; background: linear-gradient(270deg, ' + cfg.background + ', transparent); }';
  }

  function measureAndStart() {
    textWidth = Math.round(spanA.getBoundingClientRect().width);
    if (textWidth > 0) {
      scrollPos = 0;
      lastTime = 0;
      if (animId) cancelAnimationFrame(animId);
      animId = requestAnimationFrame(tick);
    }
  }

  function applyText(text, cfg) {
    applyConfig(cfg || currentCfg || DEFAULTS);
    spanA.textContent = text;
    spanB.textContent = text;
    measureAndStart();
  }

  function tick(now) {
    if (!lastTime) lastTime = now;
    var dt = now - lastTime;
    lastTime = now;
    scrollPos -= pxPerSec * dt / 1000;
    if (scrollPos <= -textWidth) scrollPos += textWidth;
    roller.style.transform = 'translate3d(' + scrollPos + 'px, 0, 0)';
    animId = requestAnimationFrame(tick);
  }

  function loadData(cb) {
    var prev = document.querySelector('script[data-ticker]');
    if (prev) prev.remove();
    var s = document.createElement('script');
    s.setAttribute('data-ticker', '');
    s.src = 'data.js?_=' + Date.now();
    s.onload = function () { if (window.rssData) cb(window.rssData); };
    document.body.appendChild(s);
  }

  loadData(function (d) {
    var cfg = d.config || DEFAULTS;
    if (d.text) applyText(d.text, cfg);
  });

  setInterval(function () {
    loadData(function (d) {
      if (!d || !d.text) return;
      var cfg = d.config || DEFAULTS;
      var textChanged = d.text !== spanA.textContent;
      var cfgChanged = JSON.stringify(cfg) !== JSON.stringify(currentCfg);
      if (textChanged) {
        applyText(d.text, cfg);
      } else if (cfgChanged) {
        applyConfig(cfg);
      }
    });
  }, REFRESH_MS);
})();

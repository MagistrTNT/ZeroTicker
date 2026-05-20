(function () {
  'use strict';

  var REFRESH_MS = 60000;
  var DATA_URL = 'news.js?_=' + Date.now();

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
  var pxPerSec = 60;
  var lastTime = 0;
  var currentCfg = {};

  function applyConfig(cfg) {
    if (!cfg) return;
    currentCfg = cfg;
    pxPerSec = cfg.speed || 60;
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

  function applyText(text) {
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
    roller.style.transform = 'translate3d(' + Math.round(scrollPos) + 'px, 0, 0)';
    animId = requestAnimationFrame(tick);
  }

  function loadData(cb) {
    var prev = document.querySelector('script[data-ticker]');
    if (prev) prev.remove();
    var s = document.createElement('script');
    s.setAttribute('data-ticker', '');
    s.src = DATA_URL;
    s.onload = function () { if (window.rssData) cb(window.rssData); };
    document.body.appendChild(s);
  }

  function init() {
    applyConfig(window.tickerConfig);
    loadData(function (d) {
      if (d && d.text) applyText(d.text);
    });
  }

  init();

  setInterval(function () {
    var cfgChanged = window.tickerConfig && JSON.stringify(window.tickerConfig) !== JSON.stringify(currentCfg);
    if (cfgChanged) applyConfig(window.tickerConfig);

    loadData(function (d) {
      if (!d || !d.text) return;
      if (d.text !== spanA.textContent) applyText(d.text);
    });
  }, REFRESH_MS);
})();

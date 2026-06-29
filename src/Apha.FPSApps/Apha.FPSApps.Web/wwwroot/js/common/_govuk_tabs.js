document.addEventListener("DOMContentLoaded", function () {
  function activateTab(tab, tabsWrapper) {
    if (!tab || !tabsWrapper) {
      return;
    }

    /* Remove the selected state from all tabs in this wrapper. */
    tabsWrapper
      .querySelectorAll(".govuk-tabs__list-item")
      .forEach(function (item) {
        item.classList.remove("govuk-tabs__list-item--selected");
      });

    /* Hide every panel before showing the requested target panel. */
    tabsWrapper
      .querySelectorAll(".govuk-tabs__panel")
      .forEach(function (panel) {
        panel.classList.remove("active");
        panel.classList.add("govuk-tabs__panel--hidden");
      });

    /* Mark the clicked tab as selected. */
    tab.parentElement.classList.add("govuk-tabs__list-item--selected");

    const target = document.querySelector(tab.getAttribute("href"));

    /* Reveal the matching panel for the active tab. */
    if (target) {
      target.classList.add("active");
      target.classList.remove("govuk-tabs__panel--hidden");
    }
  }

  document.querySelectorAll(".govuk-tabs").forEach(function (tabsWrapper) {
    const tabs = tabsWrapper.querySelectorAll(".govuk-tabs__tab");

    tabs.forEach(function (tab) {
      tab.addEventListener("click", function (e) {
        e.preventDefault();
        activateTab(tab, tabsWrapper);
      });
    });

    /* Activate the pre-selected tab, or fall back to the first tab. */
    const selectedTab = tabsWrapper.querySelector(
      ".govuk-tabs__list-item--selected .govuk-tabs__tab",
    );
    const defaultTab =
      selectedTab || tabsWrapper.querySelector(".govuk-tabs__tab");
    activateTab(defaultTab, tabsWrapper);
  });
});

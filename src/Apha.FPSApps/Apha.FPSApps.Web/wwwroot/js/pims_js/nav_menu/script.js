function btn() {
  if (document.getElementById("userdropdownbtn") !== null) {
    document
      .getElementById("userdropdownbtn")
      .addEventListener("click", function () {
        //const dp = document.getElementById('userdropdownbtn');
        const dp = this.closest(".userdropdownbtn");
        dp.parentElement.nextElementSibling.classList.toggle("show");
        //dp.nextElementSibling.classList.toggle('show');
        //  dp.classList.remove('show');
      });
  }
}
function initMenu() {
  document.querySelectorAll(".dropdown-toggle").forEach((toggle) => {
    toggle.addEventListener("click", function (e) {
      e.stopPropagation();

      const dropdownId = this.getAttribute("data-dropdown");
      const dropdownMenu = document.getElementById(dropdownId);
      const parentDropdown = this.closest(".dropdown");

      const btnRect = toggle.getBoundingClientRect();
      const menuRect = dropdownMenu.getBoundingClientRect();
      const vw = window.innerWidth;
      const vh = window.innerHeight;

      dropdownMenu.style.top = "";
      dropdownMenu.style.bottom = "";
      dropdownMenu.style.left = "";
      dropdownMenu.style.right = "";

      if (btnRect.bottom + menuRect.height > vh) {
        dropdownMenu.style.bottom = `${toggle.offsetHeight}px`;
      } else {
        dropdownMenu.style.top = `${toggle.offsetHeight}px`;
      }

      if (btnRect.left + menuRect.width > vw) {
        dropdownMenu.style.right = "0";
      } else if (btnRect.right - menuRect.width < 0) {
        dropdownMenu.style.left = "0";
      } else {
        dropdownMenu.style.left = "0";
      }

      document.querySelectorAll(".dropdown-menu").forEach((m) => {
        if (m !== dropdownMenu) m.classList.remove("show");
      });
      document.querySelectorAll(".dropdown").forEach((d) => {
        if (d !== parentDropdown) d.classList.remove("active");
      });

      dropdownMenu.classList.toggle("show");
      parentDropdown.classList.toggle("active");

      if (!dropdownMenu.classList.contains("show")) return;
    });
  });

  document.addEventListener("click", () => {
    document.querySelectorAll(".dropdown-menu").forEach((menu) => {
      menu.classList.remove("show");
    });
    document.querySelectorAll(".dropdown").forEach((dropdown) => {
      dropdown.classList.remove("active");
    });
  });

  document.querySelectorAll(".dropdown-item").forEach((item) => {
    item.addEventListener("click", function () {
      const dropdownMenu = this.closest(".dropdown-menu");
      const parentDropdown = this.closest(".dropdown");

      dropdownMenu.classList.remove("show");
      parentDropdown.classList.remove("active");
    });
  });
}

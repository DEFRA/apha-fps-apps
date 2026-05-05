const htmlmenu = `
    <nav class="navbar navbar-expand-lg main-nav">
        <div class="container-fluid">

            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNavDropdown" aria-label="Toggle navigation menu" aria-controls="navbarNavDropdown" aria-expanded="false">
                <span class="navbar-toggler-icon"></span>
            </button>

          <div class="collapse navbar-collapse" id="navbarNavDropdown">
                <ul class="navbar-nav">

                
				  <li class="nav-item"><a class="nav-link" asp-area="Costbook" asp-controller="Projects" asp-action="Index"">Choose Project </a></li>
				  
                  
					 <li class="nav-item">
                        <a class="nav-link" href="#">Maintainance</a>
                    </li>

                   

                </ul>
            </div>
        </div>
    </nav>
`;

function btn() {
    const userBtn = document.getElementById('userdropdownbtn');
    if (!userBtn) {
        return;
    }

    userBtn.addEventListener('click', function (event) {
        event.stopPropagation();
        const menu = document.querySelector('.userdropdown');
        if (menu) {
            menu.classList.toggle('show');
        }
    });
}

function initMenu() {
    const dropdownToggles = document.querySelectorAll('#header .dropdown-toggle');

    dropdownToggles.forEach(function (toggle) {
        toggle.addEventListener('click', function (event) {
            event.preventDefault();
            event.stopPropagation();

            const parentDropdown = this.closest('.dropdown');
            const dropdownMenu = parentDropdown ? parentDropdown.querySelector(':scope > .dropdown-menu') : null;

            if (!parentDropdown || !dropdownMenu) {
                return;
            }

            document.querySelectorAll('#header .dropdown-menu').forEach(function (menu) {
                if (menu !== dropdownMenu && !menu.contains(dropdownMenu)) {
                    menu.classList.remove('show');
                }
            });

            document.querySelectorAll('#header .dropdown').forEach(function (dropdown) {
                if (dropdown !== parentDropdown && !dropdown.contains(parentDropdown)) {
                    dropdown.classList.remove('active');
                }
            });

            dropdownMenu.classList.toggle('show');
            parentDropdown.classList.toggle('active');
            this.setAttribute('aria-expanded', dropdownMenu.classList.contains('show') ? 'true' : 'false');
        });
    });

    document.addEventListener('click', function () {
        document.querySelectorAll('#header .dropdown-menu').forEach(function (menu) {
            menu.classList.remove('show');
        });
        document.querySelectorAll('#header .dropdown').forEach(function (dropdown) {
            dropdown.classList.remove('active');
        });
    });
}

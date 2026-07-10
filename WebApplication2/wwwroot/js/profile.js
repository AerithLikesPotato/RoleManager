function toggleDropdown(event) {
            event.stopPropagation();
            const menu = document.getElementById('myDropdown');
            if (!menu) return;

            const isOpen = menu.classList.contains('show');
            document.querySelectorAll('.dropdown-menu.show').forEach((item) => item.classList.remove('show'));
            if (!isOpen) {
                menu.classList.add('show');
            }
        }

        document.addEventListener('click', function () {
            document.querySelectorAll('.dropdown-menu.show').forEach((item) => item.classList.remove('show'));
        });
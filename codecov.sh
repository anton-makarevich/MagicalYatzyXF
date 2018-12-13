curl -s https://codecov.io/bash > codecov
chmod +x codecov
./codecov -f "Tests/MagicalYatzyTests/coverage.opencover.xml" -t $CODECOV
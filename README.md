# Litdex.Security.RNG.TRNG

[Litdex.Security.RNG.TRNG](https://github.com/Shiroechi/Litdex.Security.RNG.TRNG) is expasion for [Litdex.Security.RNG](https://github.com/Shiroechi/Litdex.Security.RNG). It provide True Random Generator(TRNG) from third-party server.

# What is difference of PRNG and TRNG

## PRNG

PRNG rely on mathematical algorithms to generate *random* numbers. All pseudorandom number generators rely on a seed to generate the random sequences. This means that anybody who has access to the seed will be able to generate the same sequence of random numbers.

Moreover, most pseudorandom numbers have a finite period. Good pseudorandom number generators, for example the Mersenne Twister MT19937 have humongous periods. But eventually, if we wait long enough, the sequence will repeat itself.

## TRNG

With TRNG, even if two exactly identical generators were placed in identical environments with identical initial conditions, the two streams of number generated will still be totally uncorrelated to each other.

# Download

[![Nuget](https://img.shields.io/nuget/v/Litdex.Security.RNG?label=Litdex.Security.RNG&style=for-the-badge)](https://www.nuget.org/packages/Litdex.Security.RNG)

[![Nuget](https://img.shields.io/nuget/v/Litdex.Security.RNG.TRNG?label=Litdex.Security.RNG.TRNG&style=for-the-badge)](https://www.nuget.org/packages/Litdex.Security.RNG.TRNG)

# Third party servers:

- [Ligos](https://makemeapassword.ligos.net/)

From this [site](https://blog.ligos.net/2017-05-08/Building-A-CRNG-Terninger-1-Introduction.html) `makemeapassword.ligos.net` using [Fortuna](https://www.schneier.com/academic/paperfiles/fortuna.pdf) CSPRNG with various source to supplement the entropy. So it can be said truly random.

- [ANU](https://qrng.anu.edu.au

ANU is The Australian National University. They use these random numbers to generate secret keys for their quantum cryptography experiments.

# How to use

For detailed use read [How to use](https://github.com/Shiroechi/Litdex.Security.RNG/wiki/How-to-use)
or [Documentation](https://github.com/Shiroechi/Litdex.Security.RNG/wiki/Documentation)

The simple way to use

```C#
// create rng object
var rng = new ANU();

// get random integer
var randomInt = rng.NextInt();
```

# Contribute

Feel free to open new [issue](https://github.com/Shiroechi/Litdex.Security.RNG.TRNG/issues) or [PR](https://github.com/Shiroechi/Litdex.Security.RNG.TRNG/pulls).

# Donation

Like this library? Please consider donation

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/X8X81SP2L)
